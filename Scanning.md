Here's the exact Blender workflow, step by step:                                                                                                       
                                                                                                                                                       
  ---                                                                                                                                                    
  In Blender  
                                                                                                                                                         
  1. Clean up the scan

  - Delete any background/surface that got captured with the hold
  - Remove loose floating geometry: Mesh > Clean Up > Delete Loose
  - Fill any obvious holes: select border edges → Alt+Click → F

  2. Decimate

  - Add a Decimate modifier
  - Target 5,000–10,000 faces (drag the Ratio down and watch the face count in the top-left)
  - Apply the modifier
  - Check for bad normals: turn on Face Orientation overlay (blue = good, red = flipped) — fix with Alt+N > Recalculate Outside

  3. Set the origin to the mounting face

  This is the most important step for Unity placement to feel right.

  - Rotate the hold so the flat mounting face (where the bolt goes) is facing down (-Y in Blender / -Z in Unity)
  - Select the face(s) of the mounting surface → Shift+S > Cursor to Selected
  - Then Object > Set Origin > Origin to 3D Cursor

  Now the origin sits on the wall-contact surface of the hold. When you place it in Unity, position (0,0,0) relative to the wall will be flush.

  4. Make the collision mesh

  - Duplicate the object (Shift+D, Esc)
  - Add a second Decimate modifier, push it down to ~300–500 faces
  - Apply it — this is your collision mesh, it doesn't need to look good
  - Rename it HoldName_col (Unity recognizes the _col suffix and treats it as a collision mesh automatically when using the FBX importer)

  5. Scale and export

  - Make sure scale is applied: Ctrl+A > All Transforms
  - Measure the hold against a real-world reference — use N panel > Item to check dimensions in meters (a typical hold is 0.08–0.15m wide)
  - Select both meshes, File > Export > FBX
    - Set Scale: 1.0
    - Forward: -Z Forward, Up: Y Up (Unity standard)
    - Check Selected Objects only

  ---
  In Unity

  1. Drop the FBX into Assets/Holds/Meshes/
  2. Click the FBX in the Project window → Inspector > Model tab
    - Verify the scale looks right in the preview
  3. Click Materials tab → assign your shared hold material
  4. Drag the mesh from the Project window into the scene to verify it looks correct and sits flush on a flat surface
  5. Make a prefab: drag it into Assets/Holds/Prefabs/

  Once that prefab looks right, then create the HoldData ScriptableObject for it. No point building the data layer until you've confirmed the mesh
  pipeline works.

  ---
  What does your current scan look like in Blender — is it already cleaned up or still raw from RealityScan?

