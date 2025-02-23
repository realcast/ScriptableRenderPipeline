# Creating and editing Materials that use HDRP Shader Graphs

The High Definition Render Pipeline (HDRP) uses Unity's [Shader Graph](<https://docs.unity3d.com/Packages/com.unity.shadergraph@latest/index.html?preview=1>) for all of its Shaders, except the AxF Shader. This means that, for [some Materials](#MaterialList), you do not create and edit them in the same way as normal Materials.

<a name="MaterialList"></a>

## HDRP Materials that use Shader Graph

HDRP includes the following Shader Graphs:

- Decal
- Fabric
- Hair
- Lit
- StackLit
- Unlit

The [Lit](Lit-Shader), LayeredLit, and [Unlit](Unlit-Shader) Shaders are available as standard Shaders (without Shader Graph). This means that you can use them without creating a Shader Graph instance, and edit them in the Inspector. To use these, select a Material to view it in the Inspector and click on the **Shader** drop-down. Go to the **HDRP** section to see a list of every HDRP Shader that does not require a Shader Graph instance.

<a name="Creation"></a>

## Creation

To create a Material that uses a Shader Graph (for example, a StackLit Graph), follow these steps:

1. Create a Shader with the Shader Graph that you want the Material to use.
	1. Go to **Assets > Create > Shader > HDRP** to find the list of HDRP Shader Graphs. For this example, click **StackLit Graph**.
   1. Give the Shader Graph a unique name. This is important, because you need to reference this Shader Graph in the Material.
1. Create a Material from the Shader.
1. In your Project window, find the Shader that you just created and right-click it.
   1. Select **Create > Material**. This creates a Material that uses the Shader you selected. It is very important to do it this way; do not create a default Material and then select the Shader in the Material's **Shader** drop-down. For information on why, see [Known issues](#KnownIssues).
   1. Give the Material a name and press *Return* on your keyboard.

## Editing

To edit properties for Materials that use Shader Graphs, the Inspector window only allows access to a limited number of properties. To edit all Material properties, you must directly edit the Shader Graph's Master Node.

1. Double-click on the Shader Graph Asset to open it. The window displays the Master Node and a list of the available inputs. See these in the **Surface Inputs** section of the screenshot below.
2. To expose the rest of the properties, click on the cog in the top right of the Master Node. See these other properties in the **Surface Options** section of the screenshot below.
3. Edit the values for the cog's properties in the same way as you would do in the Inspector window. The list of inputs on the Master Node, and the available properties in the cog's list, changes depending on what options you select.

![](Images/CreatingAndEditingHDRPShaderGraphs1.png))

<a name="KnownIssues"></a>

## Known issues

- When you create a new Material, it must inherit from the default Shader Graph properties (in the Master Node settings). To do this, you must use the method described in the [Creation](#Creation) section. If you don’t do this, and instead use **Assets > Create > Material** to create a Material, Unity assigns the Lit Shader to the Material and then writes all the default Lit Shader properties to it. This means that, when you assign the Shader Graph to the Material, the Material uses the default properties from the Lit shader instead of those from the Shader Graph Master Node.

- When you modify the properties of the Master Mode in a Shader Graph, Unity does not synchronize them with Materials unless the Material is open in the Inspector. This means that, when you change certain properties in the Master Node settings, like **Material Type**, Materials that are not open in the Inspector fall out of sync, which breaks the rendering of these Materials. To fix the rendering and synchronize the properties of a Material with its Shader Graph, you can do the following:

- - Open the Material in the Inspector, change the value of one of its properties, and then change it back. This triggers a sync between the master node and the Material, and fixes the Material.
  - Call `HDEditorUtils.ResetMaterialKeywords(Material)` from a C# script in your Project. This synchronizes the properties of the Material you pass in with the Material's Shader Graph.