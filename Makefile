start:
	dotnet watch --launch-profile dev

build:
	dotnet build

test:
	dotnet test

docker:
	docker build -t musetrip360_server -f Dockerfile .

new-migration:
	dotnet ef migrations add $(name) --verbose 

migration:
	dotnet ef database update

docker-install:
	docker-compose up -d

.PHONY: start build docker new-migration migration test docker-install