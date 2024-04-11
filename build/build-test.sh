rm -rf test_results

dotnet build src/  -c Release

#dotnet test \
#  --filter exclude!="true" \
#  --no-build  \
#  --logger:"trx;LogFileName=test-results.trx" \
#  -c Release  \
#  /p:ExcludeByAttribute=\"Obsolete,ExcludeFromCodeCoverage\"  \
#  /p:MergeWith=\"../../test_results/coverage.json\"  \
#  --results-directory ./test_results \
#  --collect:"XPlat Code Coverage"  \
#  --verbosity normal 
  
# delete duplicate coverage
rm -rf test_results/**$(date +'%Y')**