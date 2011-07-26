:install
r:

\stackhash\buildtools\servicectrl StackHashService Stop
cd \stackhash\servicecontracts\serviceinstall

installutil stackhashservice.exe

netsh http add urlacl url=http://+:8000/StackHash user=NetworkService
netsh http add urlacl url=http://+:8001/StackHash user=NetworkService

cd \stackhash\codeplex
\stackhash\buildtools\servicectrl StackHashService Start


