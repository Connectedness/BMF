#!/usr/bin/env bash

set -euo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "$script_directory/.." && pwd)"
light_guard_clauses_root="${1:-"$repository_root/../Light.GuardClauses"}"
light_guard_clauses_root="$(cd -- "$light_guard_clauses_root" && pwd)"
exporter_directory="$light_guard_clauses_root/tools/source-export/Light.GuardClauses.SourceCodeTransformation"
exporter_project="$exporter_directory/Light.GuardClauses.SourceCodeTransformation.csproj"
settings_file="$exporter_directory/settings.json"
source_folder="$light_guard_clauses_root/src/Light.GuardClauses"
target_file="$repository_root/src/BrilliantMessaging.Core/GuardClauses/BrilliantMessaging.GuardClauses.g.cs"

for required_path in "$light_guard_clauses_root/.git" "$exporter_project" "$settings_file" "$source_folder"; do
    if [[ ! -e "$required_path" ]]; then
        echo "Required Light.GuardClauses path does not exist: $required_path" >&2
        exit 1
    fi
done

if [[ -n "$(git -C "$light_guard_clauses_root" status --porcelain)" ]]; then
    echo "The Light.GuardClauses worktree must be clean before source generation." >&2
    exit 1
fi

selected_assertions=(
    InvalidArgument
    InvalidOperation
    MustBeAssignableTo
    MustBeConcreteClass
    MustBeIn
    MustBeGreaterThanOrEqualTo
    MustBePositive
    MustBePositiveOrInfinite
    MustBeUri
    MustBeValidEnumValue
    MustNotBeDefault
    MustNotBeDefaultOrEmpty
    MustNotBeEmpty
    MustNotBeNegative
    MustNotBeNull
    MustNotBeNullOrEmpty
    MustNotBeNullOrWhiteSpace
    MustNotBeNullReference
    MustNotContainNull
    MustNotStartWith
    ObjectDisposed
)

factory_overload_assertions=(
    MustBeUri
    MustNotBeDefault
    MustNotBeEmpty
    MustNotBeNull
    MustNotBeNullOrWhiteSpace
)

required_options=(
    AssertionWhitelist
    BaseNamespace
    ChangePublicTypesToInternalTypes
    IncludeCallerArgumentExpressionAttribute
    IncludeCodeAnalysisNullableAttributes
    IncludeJetBrainsAnnotations
    IncludeJetBrainsAnnotationsUsing
    IncludeValidatedNotNullAttribute
    IncludeVersionComment
    RemoveCallerArgumentExpressions
    RemoveContractAnnotations
    RemoveDoesNotReturn
    RemoveNoEnumeration
    RemoveNotNullWhen
    RemoveOverloadsWithExceptionFactory
    RemoveValidatedNotNull
    SourceFolder
    TargetFile
    TargetFramework
    ValidateGeneratedFileBuild
)

catalog_assertions=()
while IFS= read -r assertion; do
    catalog_assertions+=("$assertion")
done < <(jq -er '.AssertionWhitelist | keys[] | select(. != "IsEnabled")' "$settings_file")
if [[ "${#catalog_assertions[@]}" -eq 0 ]]; then
    echo "No assertion catalog entries were found in $settings_file." >&2
    exit 1
fi

contains()
{
    local expected="$1"
    shift
    local candidate
    for candidate in "$@"; do
        if [[ "$candidate" == "$expected" ]]; then
            return 0
        fi
    done
    return 1
}

for option in "${required_options[@]}"; do
    if ! jq -e --arg option "$option" 'has($option)' "$settings_file" > /dev/null; then
        echo "Required transformation option '$option' is missing from the upstream configuration." >&2
        exit 1
    fi
done

for assertion in "${selected_assertions[@]}"; do
    if ! contains "$assertion" "${catalog_assertions[@]}"; then
        echo "Selected assertion '$assertion' is missing from the upstream catalog." >&2
        exit 1
    fi
done

for assertion in "${factory_overload_assertions[@]}"; do
    if ! contains "$assertion" "${selected_assertions[@]}"; then
        echo "Factory overload assertion '$assertion' is not selected." >&2
        exit 1
    fi
done

temporary_directory="$(mktemp -d)"
trap 'rm -rf -- "$temporary_directory"' EXIT
temporary_file="$temporary_directory/BrilliantMessaging.GuardClauses.g.cs"

arguments=(
    --SourceFolder "$source_folder"
    --TargetFile "$temporary_file"
    --TargetFramework NetStandard2_0
    --ValidateGeneratedFileBuild false
    --ChangePublicTypesToInternalTypes false
    --BaseNamespace BrilliantMessaging.GuardClauses
    --RemoveContractAnnotations true
    --IncludeJetBrainsAnnotations true
    --IncludeJetBrainsAnnotationsUsing true
    --RemoveNoEnumeration false
    --IncludeVersionComment true
    --RemoveOverloadsWithExceptionFactory false
    --IncludeCodeAnalysisNullableAttributes false
    --IncludeValidatedNotNullAttribute false
    --RemoveValidatedNotNull true
    --RemoveDoesNotReturn false
    --RemoveNotNullWhen false
    --IncludeCallerArgumentExpressionAttribute false
    --RemoveCallerArgumentExpressions false
    --AssertionWhitelist:IsEnabled true
)

for assertion in "${catalog_assertions[@]}"; do
    include=false
    include_factory=false
    if contains "$assertion" "${selected_assertions[@]}"; then
        include=true
    fi
    if contains "$assertion" "${factory_overload_assertions[@]}"; then
        include_factory=true
    fi
    arguments+=(
        "--AssertionWhitelist:$assertion:Include" "$include"
        "--AssertionWhitelist:$assertion:IncludeExceptionFactoryOverload" "$include_factory"
    )
done

(
    cd -- "$exporter_directory"
    dotnet run --project "$exporter_project" -- "${arguments[@]}"
)

source_version="$(
    sed -n 's:.*<Version>\([^<]*\)</Version>.*:\1:p' "$light_guard_clauses_root/Directory.Build.props" |
        head -n 1
)"
source_commit="$(git -C "$light_guard_clauses_root" rev-parse HEAD)"
if [[ -z "$source_version" || -z "$source_commit" ]]; then
    echo "Could not determine Light.GuardClauses source provenance." >&2
    exit 1
fi

sed -i.bak \
    "2i\\
   Source: Light.GuardClauses ${source_version}, commit ${source_commit}
" \
    "$temporary_file"
rm -- "$temporary_file.bak"

mkdir -p -- "$(dirname -- "$target_file")"
mv -- "$temporary_file" "$target_file"
echo "Generated $target_file from Light.GuardClauses $source_version ($source_commit)."
