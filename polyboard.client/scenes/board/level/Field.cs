using Godot;
using System;
using System.Collections.Generic;

public partial class Field : Node3D
{
	protected MeshInstance3D fieldMeshInstance;
	protected Vector3 bRCL; //dolny prawy rog - lokalna pozycja
	protected Vector3 bRCG; //dolny prawy rog - globalna pozycja
	public List<Vector3> positions=new List<Vector3>(6);
	protected static int nextId = 0;
	public int FieldId;

	public Field()
	{
		FieldId = nextId;
		nextId++;
	}
	public override void _Ready()
	{
		SetPositions();
		
	}
	public virtual void SetPositions()
	{
		fieldMeshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		
		if (fieldMeshInstance != null)
		{
			
			
			var mesh = fieldMeshInstance.Mesh;
				var boundingBox = mesh.GetAabb();
				bRCL = boundingBox.Position + boundingBox.Size;
				bRCG = fieldMeshInstance.ToGlobal(bRCL);

			
			if(FieldId>=1 && FieldId<=9)
			{
				positions.Add(new Vector3(bRCG.X+2.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X+2.5f, 0.5f, bRCG.Z-0.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z-0.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z-0.5f));
				
			}
			else if(FieldId>=11 && FieldId<=19)
			{
				
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+2.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+2.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X+1.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X+0.5f, 0.5f, bRCG.Z+0.5f));
			}
			else if(FieldId>=21 && FieldId<=30)
			{
				positions.Add(new Vector3(bRCG.X-2.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-2.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z+0.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z+1.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z+0.5f));
			}
			else if(FieldId>=31 && FieldId<=40)
			{
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-2.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-2.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-1.5f));
				positions.Add(new Vector3(bRCG.X-1.5f, 0.5f, bRCG.Z-0.5f));
				positions.Add(new Vector3(bRCG.X-0.5f, 0.5f, bRCG.Z-0.5f));
			}
				
				
				
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Any additional logic that you want to execute every frame
	}
}
