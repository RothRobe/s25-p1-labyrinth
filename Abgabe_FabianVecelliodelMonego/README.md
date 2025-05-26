Labyrinth UE5 Project – README.txt

1. Prerequisites
   • Windows 10 or 11  
   • NVIDIA GPU with hardware Ray-Tracing support (from RTX 20 upwards)  
   • Epic Games Launcher (to install Unreal Engine 5.5 or later)  
   • JetBrains Rider (with the UnrealLink plugin enabled)  

2. Setup
   Ensure the Maze file Level1.txt resides here:  
   	Content/Mazes/Level1.txt  
   (32 lines of 32 characters: ‘0’ = empty, ‘1’/‘2’/‘3’ = wall types; corners must remain ‘0’.)  

3. Open & Build
   1. Double-click **Labyrinth.uproject** to open in Unreal Editor.  
   2. In Rider, open the generated solution (`Labyrinth.sln`).  
   3. Build → **Build Solution** (Ctrl+Shift+B).  
   4. In Rider’s toolbar, click **Launch Unreal Editor** (top-right).  

4. Play
   • In Unreal Editor, click the **Play** button (top bar).  
   • Use WASD + mouse to navigate; shoot with left click.  

5. Live-Coding & Iteration
   • After changing C++ code, press **Ctrl+Alt+F11** in Rider to hot-reload.  
   • Modify `Level1.txt` at runtime; then click **Play** to see updated maze.  

6. Troubleshooting
   • If build fails, delete the folders **Binaries**, **Intermediate** and **Saved**, then rebuild.
	-> in this case some texture scaling problems may occure 
   • Update your GPU drivers to the latest version.  
   • Make sure UnrealLink is enabled in Rider (Settings → Plugins). 