using UnityEngine;
using System.Collections.Generic;
using MapMagic;
//using Plugins;

[System.Serializable]
//[GeneratorMenu (menu="MyGenerators", name ="MyGenerator", disengageable = true)]
public class MyCustomGenerator : Generator
{
    //input and output properties
    public Input input = new Input(InoutType.Map);
    public Output output = new Output(InoutType.Map);

    //including in enumerator
    public override IEnumerable<Input> Inputs() { yield return input; }
    public override IEnumerable<Output> Outputs() { yield return output; }
    public float level = 1;
    public override void Generate (MapMagic.Chunk chunk, Biome biome=null)
    {
        Matrix src = (Matrix)input.GetObject(chunk);

        if (src==null || chunk.stop) return;
           if (!enabled) { output.SetObject(chunk, src); return; }

        Matrix dst = new Matrix(src.rect);

        Coord min = src.rect.Min; Coord max = src.rect.Max;

        for (int x=min.x; x<max.x; x++)
           for (int z=min.z; z<max.z; z++)
		{
              float val = level - src[x,z];
              dst[x,z] = val>0? val : 0;
		}

        if (chunk.stop) return;
        output.SetObject(chunk, dst); 
    }

    public override void OnGUI ()
    {
        layout.Par(20); input.DrawIcon(layout, "Input", mandatory:true); output.DrawIcon(layout, "Output");
        layout.Field(ref level, "Level", min:0, max:2);

    }

}