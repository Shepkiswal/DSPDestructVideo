# DSPDestructVideo
### 【戴森球计划Mod】
利用传送带播放视频

### 【注意事项】
用VS打开后，请先修改项目属性->生成文件

taskkill /f /im DSPGAME.exe  
mkdir "``D:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\``BepInEx\plugins\$(ProjectName)"  
del /q "``D:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program``\BepInEx\plugins\$(ProjectName)\$(TargetFileName)"  
copy "$(TargetPath)" "``D:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program``\BepInEx\plugins\$(ProjectName)\$(TargetFileName)"  
start steam://rungameid/1366540

请将上方引用的部分替换为你戴森球计划的根目录，并粘贴到生成后事件命令行中

