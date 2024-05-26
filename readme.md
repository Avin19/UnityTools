# Unity Tools for Esay Update / Delete / Add Package Manager

### I am Keeping the Scripts that i need to  add here so it would be easy .


# Run these code in the root folder of the Unity 

### Generate the folder by going to Tool> Setup > Create Default Folders


### MAC 


```bash
touch .gitignore
curl -o .gitignore https://raw.githubusercontent.com/w3villa-avinash/UnityTool/main/.gitignore
curl -OJL https://raw.githubusercontent.com/w3villa-avinash/UnityTool/main/Tools.cs


```
### Windows 

```bash

curl -o .gitignore https://raw.githubusercontent.com/Avin19/UnityTools/main/.gitignore
curl -o Assets\Scritps\Tools.cs https://raw.githubusercontent.com/Avin19/UnityTools/main/Tools.cs 
```


PlantUml Diagram Generator 

puml-gen C:\Source\App1 C:\PlantUml\App1 -dir -ignore Private,Protected -createAssociation -allInOne
puml-gen {Input/Scripts/} {Out/plantuml} -dir -ignore Private,protected -createAssociation -allInOne 


puml-gen C:\Source\App1 C:\PlantUml\App1 -dir -excludePaths bin,obj,Properties


For more information take a look at 
![Click Here](https://github.com/pierre3/PlantUmlClassDiagramGenerator)



Editior Scritps 

```bash 
curl -o Assets\Editor\CustomScritpsTemplate.cs https://raw.githubusercontent.com/Avin19/UnityTools/main/CustomScriptsTemplate.cs

```