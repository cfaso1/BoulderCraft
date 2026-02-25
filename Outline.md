# Outline

**Goal**: Build a shareable windows/mac/linux application designed for route setters to easily plan and map out routes in a 3D mapped environment of a real gym using a large selection of 3D scanned holds and volumes. 

## Environment
- A basic room with actual climbing walls mapped out based on BlueSwan Boulders climbing gym
- Wall design
    - Grey scale for simplicity and realism
    - Slightly diffrent shades to indicate angle change
    - Holes for screw anchors and grid snapping
    - Basic thick blue stripes to break pattern and orient player
- The walls will include differnt variations
    - Angles: vertical (90 degress) overhangs (20-45 degrees) roof (0 degrees) and slabs (-5 degrees)
    - Inside and outside edges/corners
    - Walls with multiple angles (zig-zag vertically/horizontally)
- The walls will start blank with no holds
- Player will place 3D scanned holds onto the walls from inventory
- Many light sources to minimize but still have some shadows
- Mats, floor, and ceiling to model gym layout

## Player Controls
- Player is restricted to the climbing room and cannot go through the walls
- Free cam movement
    - Slow-Medium speed flight to ensure precision
    - Mouse -> Move camera direction
    - WASD -> Move player in direction
    - Space -> Player up
    - Shift -> Player down
- Placing holds
    - Dragged from side menu onto the wall in front of player
    - Can drag existing holds to edit position
    - Clicking on hold will show a rotation slider
    - Right clicking will show delete, lock/unlock, and start/top toggles
    - Holds can snap to grid or be free placed (toggleable option)
    - Holds cannot be placed overlapping, off the edge, or inside a wall
    - Arrow keys -> precise nudging of holds

## Player UI
- Save button (Provide name if first save)
- Import button (Opens file folder for user drag and drop)
- Help button for list of controls
- Home button (To open other projects or quit)
- Teleport button with dropdown of set locations in gym
- Holds menu
    - Large scrollable list of holds with a picture of hold
    - Can be filtered by color, type, and size
    - Search bar at the top
    - Can be hidden while designing
- Current/previous hold will be outlined bold (clicking elsewhere will remove it)
- Right click menu will show options on mouse (clicking elsewhere will remove it)

## Physics
- Player cannot fly through walls, floors, or ceilings
- Holds cannot be placed where their base hangs off the edge of wall
- Holds cannot be placed where they clip into another hold or wall
    - Holds must be able to be placed very close to eachother
    - Checks must occur on rotation as well as placement/adjustment
- Rotating hold occurs on same plane as hold
- Holds can be snapped to a grid for easy place or nudged for precision
- Dragging a hold accross the wall will snap it to the closest valid position without chaning rotation
- Prevents player from rotating a hold into an invalid placement
- Volumes:
    - Can be placed and rotated like holds
    - Must snap to grid
    - Holds can be placed ontop of them
    - Extend the valid placements for holds
    - Can be marked as start/top

## Hold Database
- Done through Unity assests and prefabs
- Holds are stored as 3D objects
- Each hold has:
    - Unique hold ID
    - Color
    - Type (crimp, jug, slopper)
    - Size measurement
    - The 3D object file
- Around 1000 holds stored
- Saves stored as JSON

## Hold Scanning
- 3D scanned using photos
- Hand measured for size
- Texture and no-tex must be scanned aswell
- Shape/size over detail

## Saving and Sharing
- Player can save their current route
    - Will save the position and rotation of all holds set
    - Saves are given a name
- Player choses from different saved projects
- Player can import routes from previous saves or other player's save
- Importing will overwrite any collisions on current project
    - Notify player of any overwritten holds on import
    - Player imports by dragging a save file into the saves folder
    - Open saves folder button for ease of access

## Application sharing
- Application available on Windows/Mac/Linux
- Application is stored in one folder
- User can add a shortcut to their desktop
- Open source on github for free download

## Tech stack
- Unity for gameplay and physics (C#)
- Blender for creating and refining 3D models
- RealityScan for scanning holds into 3D models
- GitHub for version control and sharing

## Demo
- Player movement
- Basic wall placement physics
- Holds inventory
- Holds placement
- 3D scan proof of concept
- Wall texture/drill holes
