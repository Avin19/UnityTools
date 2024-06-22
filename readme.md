# Unity Tools for Esay Update / Delete / Add Package Manager

### I am Keeping the Scripts that i need to  add here so it would be easy .


# Run these code in the root folder of the Unity 

### Generate the folder by going to Tool> Setup > Create Default Folders

<!-- 
### MAC 


```bash
touch .gitignore
curl -o .gitignore https://raw.githubusercontent.com/w3villa-avinash/UnityTool/main/.gitignore
curl -OJL https://raw.githubusercontent.com/w3villa-avinash/UnityTool/main/Tools.cs


``` -->
### Windows 

```bash

curl -o .gitignore https://raw.githubusercontent.com/Avin19/UnityTools/main/.gitignore
curl -o Assets\Tools.cs https://raw.githubusercontent.com/Avin19/UnityTools/main/Tools.cs 
```

### Editior Scritps 

```bash 

mkdir  Assets\Project\Editor\Template
cd  Assets\Project\Editor\
curl -o Assets\Project\Editor\CustomScritpsTemplate.cs https://raw.githubusercontent.com/Avin19/UnityTools/main/CustomScriptsTemplate.cs
curl -O Template\NewScript.cs.txt https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewScript.cs.txt 
curl -O Template\NewEnum.cs.txt https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewEnum.cs.Txt
curl -O Template\NewScriptableObject.cs.txt https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewScriptableObject.cs.txt
curl -O Template\NewClass.cs.txt https://raw.githubusercontent.com/Avin19/UnityTools/main/Template/NewClass.cs.txt
```

### PlantUml Diagram Generator 

puml-gen C:\Source\App1 C:\PlantUml\App1 -dir -ignore Private,Protected -createAssociation -allInOne
puml-gen {Input/Scripts/} {Out/plantuml} -dir -ignore Private,protected -createAssociation -allInOne 


puml-gen C:\Source\App1 C:\PlantUml\App1 -dir -excludePaths bin,obj,Properties


For more information take a look at 
![Click Here](https://github.com/pierre3/PlantUmlClassDiagramGenerator)






## To Do List 

- [ ] Reduce the Number of Setup using bash script 
- [ ] Once Everything is done remove all the Tools relate scripts 


current directory Directory.GetCurrentDirectory()