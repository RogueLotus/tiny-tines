# Tiny-Tines Coding Project


## Setup Prerequisites
* .Net Core SDK (Version 5.0.201)

#### Note:
The project was created in Visual Studio. It uses:

* C# extension for VS.
* Frameworks:
  * NUnit Version 3.12.3
  * MS Test Version 2.1.1


## Run Project and Tests
Unzip source code folder *tiny-project*.



### 1. Visual Studio
Open Visual Studios. Go to ```File -> Open -> Project/Solution``` and choose *tiny-tines.sln* in ```tiny-project\tiny-tines```.

####  Run Tests
```Test -> Run All Tests```
or use ``` CTRL+R, A```.


### 2. Command Prompt

Open command prompt and navigate to *tiny-tines* folder.
Run following command:

#### Run Tests
```dotnet test```

##### Debug
You can run Debug from ```tiny-project\tiny-tines\bin\Debug\net5.0```
by running ```tiny-tines.exe [FILENAME]```


#### Run Program
```dotnet run  [FILENAME]```


*Note: **Filename** should be a path to the input json file.*


