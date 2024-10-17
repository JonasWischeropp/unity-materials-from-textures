# Materials from Textures
This small editor tool makes it simple to create materials from folders of textures.  
The tool assumes that one folder contains all the textures for one material.

Custom rules can be defined to match file names and shader properties using [regular expressions](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) (not case-sensitive).

<!-- TODO insert gif -->

## How to use
1. Select the folders that should be converted into materials
2. Right-click on the selection
3. Click `Create > Material(s) from Textures`
4. Specify the shader
5. If required adjust settings
6. Click `Create Material(s)`

## Setup
Installation using the Package Manager:
1. Click on the `+` in the `Package Manager` window
2. Chose `Add package from git URL...`
3. Insert the following URL `https://github.com/JonasWischeropp/unity-materials-from-textures.git#1.0.0`  
A specific [release](https://github.com/JonasWischeropp/unity-materials-from-textures/releases) version can be specified after `#`.
4. Press the `Add`-Button

## Example folder structure
```
├── desert_ground_01_2k
│   ├── desert_ground_01_ambientOcclusion_2k.png
│   ├── desert_ground_01_baseColor_2k.png
│   ├── desert_ground_01_height_2k.png
│   ├── desert_ground_01_m_ao_h_r_2k.png
│   ├── desert_ground_01_metallic_2k.png
│   ├── desert_ground_01_normal_dx_2k.png
│   ├── desert_ground_01_normal_gl_2k.png
│   ├── desert_ground_01_roughness_2k.png
│   └── license.txt
├── stone_bricks_wall_07_2k
│   ├── license.txt
│   ├── stone_bricks_wall_07_ambientOcclusion_2k.png
│   ├── stone_bricks_wall_07_baseColor_2k.png
│   ├── stone_bricks_wall_07_height_2k.png
│   ├── stone_bricks_wall_07_m_ao_h_s_2k.png
│   ├── stone_bricks_wall_07_metallic_2k.png
│   ├── stone_bricks_wall_07_normal_dx_2k.png
│   ├── stone_bricks_wall_07_normal_gl_2k.png
│   └── stone_bricks_wall_07_roughness_2k.png
```

## Additional information
- The settings file can be found under `<Project Root>/ProjectSettings/Packages/wischeropp.jonas.materials-from-textures/Settings.asset`
- `Shader Property Name Regex` matches with the description name (e.g. `Albedo`) not the name used in the shader (e.g. `_MainTex`)
- due to the use of `MultiColumnListView` Unity 2022.1 or higher is required 
