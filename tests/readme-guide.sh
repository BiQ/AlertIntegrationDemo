#!/usr/bin/env bash
# Walks the "Getting Started" guide in README.md, so we know the guide still works.
#
# The commands below must be identical to the `$ ` lines in the README. The test
# fails if they differ, so a change to one must be done in the other too.
#
# Step 1 (writing credentials into ConfigValues.cs) is manual, so the test runs
# with the placeholder values. Output that needs a response from BiQ is not
# checked; the long-running programs are stopped once their start-up output is seen.
set -euo pipefail

repo=$(cd "$(dirname "$0")/.." && pwd)

# Commands in README order, and how many of the README's output lines to check for
# each. The checked lines must not depend on a response from BiQ.
steps=(
  'dotnet run --project CustomerSystemInitializer'
  'echo "select * from customers" | sqlite3 customersystem.db'
  'dotnet run --project ShadowWriter'
  'dotnet run --project ChangeReader'
  'dotnet run --project NotificationReader'
)
checked_lines=(0 2 1 2 2)
long_running=(no no yes yes yes)
startup_timeout_seconds=300

# Prints the guide's code blocks as "CMD<tab>command" and "OUT<tab>output line".
# Output stops at "...snip..." or at the end of the block.
readme_blocks() {
  awk '
    /^## / { in_guide = ($0 ~ /^## Getting Started/) }
    !in_guide { next }
    /^[[:space:]]*```/ { in_block = !in_block; snipped = 0; next }
    !in_block || snipped { next }
    { sub(/^[[:space:]]+/, "") }
    /^\.\.\.snip\.\.\.$/ { snipped = 1; next }
    /^\$ / { print "CMD\t" substr($0, 3); next }
    { print "OUT\t" $0 }
  ' "$repo/README.md"
}

# Prints the README output lines shown after command number $1 (0-based).
readme_output() {
  readme_blocks | awk -F '\t' -v want="$1" '
    $1 == "CMD" { n++; next }
    n - 1 == want { print $2 }
  '
}

fail() {
  echo "FAIL: $*" >&2
  exit 1
}

echo "== Check that the README commands and the test steps are identical"
if ! diff -u \
  --label README.md <(readme_blocks | awk -F '\t' '$1 == "CMD" { print $2 }') \
  --label "$0" <(printf '%s\n' "${steps[@]}"); then
  fail "README.md and the steps in $0 differ"
fi

# Run in a fresh copy, as a new developer would, so local databases and bookmark
# files do not change the result.
# Use the real path: on macOS the temp dir is under the /var -> /private/var symlink,
# and the .NET 11 SDK then restores project references under both paths and fails.
work=$(cd "$(mktemp -d)" && pwd -P)
pids=()
cleanup() {
  for pid in ${pids[@]+"${pids[@]}"}; do
    kill -- "-$pid" 2>/dev/null || true
  done
  rm -rf "$work"
}
trap cleanup EXIT
(cd "$repo" && git ls-files -z --cached --others --exclude-standard | tar --null -T - -cf -) | tar -xf - -C "$work"
cd "$work"

# Each background job gets its own process group, so the whole `dotnet run` tree can be stopped.
set -m

for i in "${!steps[@]}"; do
  step=${steps[$i]}
  expected=$(readme_output "$i" | awk -v n="${checked_lines[$i]}" 'NR <= n')
  log="$work/step-$i.log"
  echo "== \$ $step"

  if [[ ${long_running[$i]} == no ]]; then
    eval "$step" >"$log" 2>&1 || { cat "$log"; fail "'$step' exited with an error"; }
    cat "$log"
    if [[ ${checked_lines[$i]} -gt 0 ]]; then
      diff -u --label README.md <(echo "$expected") --label output <(head -n "${checked_lines[$i]}" "$log") ||
        fail "output of '$step' differs from README.md"
    fi
    continue
  fi

  eval "$step" >"$log" 2>&1 &
  pid=$!
  pids+=("$pid")
  waited=0
  while IFS= read -r line; do
    until grep -qxF -- "$line" "$log"; do
      kill -0 "$pid" 2>/dev/null || { cat "$log"; fail "'$step' stopped before it printed: $line"; }
      [[ $waited -lt $startup_timeout_seconds ]] || { cat "$log"; fail "'$step' did not print in time: $line"; }
      sleep 1
      waited=$((waited + 1))
    done
  done <<<"$expected"
  cat "$log"
  kill -- "-$pid"
  wait "$pid" 2>/dev/null || true
done

echo "== README guide OK"
