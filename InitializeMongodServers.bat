mkdir C:\mongodb\mongodb1
mkdir C:\mongodb\mongodb2
mkdir C:\mongodb\mongodb3

start cmd /k C:\mongodb\bin\mongod --dbpath C:\mongodb\mongodb1 --port 11233 --replSet rs0 --smallfiles --oplogSize 128
start cmd /k C:\mongodb\bin\mongod --dbpath C:\mongodb\mongodb2 --port 22344 --replSet rs0 --smallfiles --oplogSize 128
start cmd /k C:\mongodb\bin\mongod --dbpath C:\mongodb\mongodb3 --port 33455 --replSet rs0 --smallfiles --oplogSize 128
