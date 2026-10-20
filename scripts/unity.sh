#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/MacOS/Unity}"
if [[ ! -x "$unity_editor" ]]; then
  echo 'Defina UNITY_EDITOR com o caminho do executável Unity 6000.3.26f1.' >&2
  exit 1
fi
mkdir -p "$project_dir/Logs" "$project_dir/TestResults"
case "${1:-}" in
  test-edit)
    "$unity_editor" -batchmode -nographics -projectPath "$project_dir" -runTests -testPlatform EditMode -testResults "$project_dir/TestResults/editmode.xml" -logFile "$project_dir/Logs/test-editmode.log" ;;
  test-play)
    "$unity_editor" -batchmode -nographics -projectPath "$project_dir" -runTests -testPlatform PlayMode -testResults "$project_dir/TestResults/playmode.xml" -logFile "$project_dir/Logs/test-playmode.log" ;;
  build-web)
    "$unity_editor" -batchmode -nographics -quit -projectPath "$project_dir" -buildTarget WebGL -executeMethod CourseBuilder.BuildWeb -logFile "$project_dir/Logs/build-webgl.log" ;;
  *) echo 'Uso: bash scripts/unity.sh {test-edit|test-play|build-web}' >&2; exit 2 ;;
esac
