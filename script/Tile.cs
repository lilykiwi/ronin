using System;
using System.Runtime.CompilerServices;
using Godot;

[GlobalClass, Tool]
public partial class Tile : StaticBody3D
{
  [Signal] public delegate void ValueChangedEventHandler();

  [ExportCategory("Assets")] // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
  
  private CompressedTexture2D? _heightMap = null;
  [Export] public CompressedTexture2D? HeightMap
  {
    get => _heightMap;
    set => SetValue(ref _heightMap, value);
  }

  private CompressedTexture2D? _normalMap = null;
  [Export] public CompressedTexture2D? NormalMap
  {
    get => _normalMap;
    set => SetValue(ref _normalMap, value);
  }

  private ShaderMaterial? _shader = null;
  [Export] public ShaderMaterial? Shader
  {
    get => _shader;
    set => SetValue(ref _shader, value);
  }
  
  [ExportCategory("Configuration")] // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
  
  private Vector2I _tileCount = new(1, 1);
  [Export] public Vector2I TileCount
  {
    get => _tileCount;
    set => SetValue(ref _tileCount, value);
  }
  
  private float _heightScale = 1.0f;
  [Export] public float HeightScale
  {
    get => _heightScale;
    set => SetValue(ref _heightScale, value);
  }
  
  private int _chunkCount = 1;
  //[Export] public int ChunkCount
  //{
  //  get => _chunkCount;
  //  set => SetValue(ref _chunkCount, value);
  //}
  
  private Vector2I _chunkIndex = new(0, 0);
  //[Export] public Vector2I ChunkIndex
  //{
  //  get => _chunkIndex;
  //  set => SetValue(ref _chunkIndex, value);
  //}
  
  [ExportCategory("Private")] // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

  private MeshInstance3D? _meshInstance = null;
  private PlaneMesh? _mesh = null;
  private CollisionShape3D? _collisionShape = null;
  private HeightMapShape3D? _heightMapShape = null;

  // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    ValueChanged += SetupCollision;
    SetupCollision();
  }
  
  public void SetupCollision()
  {
    
    if (_heightMap is null || _normalMap is null || _shader is null) 
    {
      GD.PrintErr("HeightMap is null");
      return;
    }
    
    GD.Print("setting up CollisionShape3D");
    if (HasNode("CollisionShape3D"))
    {
      _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
      UpdateConfigurationWarnings();
    }
    else
    {
      _collisionShape = new CollisionShape3D();
      _collisionShape.Name = "CollisionShape3D";
      AddChild(_collisionShape);
      _collisionShape.Owner = GetTree().EditedSceneRoot;
      UpdateConfigurationWarnings(); // unnecessary?
    }

    GD.Print("setting up CollisionShape3D Shape from HeightMap");
    Image image = _heightMap.GetImage();
    image.Decompress();
    GD.Print(image.GetFormat());
    _heightMapShape = CreateHeightMapShape(image);
    _collisionShape.Shape = _heightMapShape;
    UpdateConfigurationWarnings();
    
    GD.Print("setting up MeshInstance3D");
    if (HasNode("MeshInstance3D"))
    {
      _meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
      UpdateConfigurationWarnings();
    }
    else
    {
      _meshInstance = new MeshInstance3D
      {
        Name = "MeshInstance3D"
      };
      AddChild(_meshInstance);
      _meshInstance.Owner = GetTree().EditedSceneRoot;
      UpdateConfigurationWarnings(); // unnecessary?
    }
    UpdateShaderParameters();
    _meshInstance.MaterialOverride = _shader;
    
    GD.Print("setting up MeshInstance3D Mesh & Shader");
    _mesh = new PlaneMesh
    {
      Size = (Vector2)_tileCount,
      SubdivideDepth = _tileCount.Y - 1, // doesn't include outer edges
      SubdivideWidth = _tileCount.X - 1  // doesn't include outer edges
    };
    _meshInstance.Mesh = _mesh;
  }
  
  public HeightMapShape3D CreateHeightMapShape(Image heightMap)
  {
    HeightMapShape3D shape = new()
    {
      // THESE VALUES ARE SWAPPED as the math works out better
      MapDepth = _tileCount.Y + 1, // edges, not subdivisions
      MapWidth = _tileCount.X + 1 // edges, not subdivisions
    };

    float[] mapData = new float[shape.MapDepth * shape.MapWidth];
    for (int x = 0; x < shape.MapWidth; x++)
    {
      for (int y = 0; y < shape.MapDepth; y++)
      {
        Vector2 pos = new((float) x / _tileCount.X, (float) y / _tileCount.Y);
        mapData[y * shape.MapWidth + x] = GetNearestPixel(pos, heightMap) * _heightScale;
      }
    }

    shape.MapData = mapData;

    return shape;
  }
  
  public static float GetNearestPixel(Vector2 pos, Image image) 
  {
    int height = image.GetHeight() - 1;
    int width = image.GetWidth() - 1;

    // pos is a value between 0 and 1 (0.5, 0.5 is the center of the image).
    // we want to get the nearest 4 pixels to this fraction. we do this by multiplying the width and height by the fraction, then rounding the result.

    Vector2I values = new(
      x: (int) MathF.Round(pos.X * width),
      y: (int) MathF.Round(pos.Y * height)
    );

    return image.GetPixel(values.X, values.Y).V;
  }
  
  public void UpdateShaderParameters()
  {
    if (_shader is null) return;
    _shader.SetShaderParameter("chunkCount", _chunkCount);
    _shader.SetShaderParameter("heightScale", _heightScale);
    _shader.SetShaderParameter("heightMap", _heightMap);
    _shader.SetShaderParameter("normalMap", _normalMap);
  }

  public void SetValue<T>(ref T? current, T? newValue, [CallerMemberName] string name = "_")
  {
    current = newValue;
    EmitSignal(nameof(ValueChanged));
  }
}
