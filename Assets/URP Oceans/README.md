# Usage Notes
Important notes to get the shaders rendering properly on in game cameras.

## Packages

Make sure you have installed the Universal RP package.

## Assets

Add the default render pipeline if you havent already.

Create the render settings in "Assets" by right clicking in Assets and do:  
- Create->  
- Rendering->  
- Universal Render Pipeline->  
- Pipeline Asset (Forward Renderer)"

Edit the .asset settings
- right click on UniversalRenderPipelineAsset.asset
- select "Properties"
- check enabled on "Depth Texture"
- check enabled on "Opaque Texture"

## Project Settings

Make sure set this pipeline in "Edit -> Project Settings"

- Quality -> Rendering -> \<render pipeline asset\>
- Graphics -> Scriptable Render Pipeline Settings -> \<render pipeline asset\>
