using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 해당 위치나 충돌체에 대한 기즈모와 선택적 텍스트를 표시할 수 있습니다.
    /// </summary>
    public class MMGizmo : MonoBehaviour 
	{
		/// <summary>
		/// the possible types of gizmos to display
		/// </summary>
		public enum GizmoTypes { None, Collider, Position }
		/// <summary>
		/// whether to display gizmos always or only when the object is selected
		/// </summary>
		public enum DisplayModes { Always, OnlyWhenSelected }

		/// <summary>
		/// the shape of the gizmo to display the position of the object
		/// </summary>
		public enum PositionModes
		{
			Point, Cube, WireCube, Sphere, WireSphere, Texture, Arrows, RightArrow, UpArrow, ForwardArrow,
			Lines, RightLine, UpLine, ForwardLine
		}
		/// <summary>
		/// what to display as text for that gizmo
		/// </summary>
		public enum TextModes { GameObjectName, CustomText, Position, Rotation, Scale, Property }
		/// <summary>
		/// when displaying a collider, whether to display a full or wire gizmo
		/// </summary>
		public enum ColliderRenderTypes { Full, Wire }

		[Header("Modes")] 
		/// if this is true, gizmos will be displayed, if this is false, gizmos won't be displayed
		[Tooltip("이것이 true이면 기즈모가 표시되고, false이면 기즈모가 표시되지 않습니다.")]
		public bool DisplayGizmo = true; 
		/// what the gizmos should represent. Collider will show the bounds of the associated collider, Position will show the position of the object 
		[Tooltip("기즈모가 무엇을 나타내야 하는지. Collider는 연결된 Collider의 경계를 표시하고 Position은 개체의 위치를 ​​표시합니다.")]
		public GizmoTypes GizmoType = GizmoTypes.Position; 
		/// whether gizmos should always be displayed, or only when selected
		[Tooltip("기즈모를 항상 표시할지 아니면 선택한 경우에만 표시할지 여부")]
		public DisplayModes DisplayMode = DisplayModes.Always;
		
		[Header("Settings")] 
		/// the color of the collider or position gizmo 
		[Tooltip("충돌체의 색상 또는 위치 기즈모")]
		public Color GizmoColor = MMColors.ReunoYellow; 
		/// the shape of the gizmo when in position mode
		[Tooltip("위치 모드에 있을 때 기즈모의 모양")]
		[MMEnumCondition("GizmoType", (int)GizmoTypes.Position)]
		public PositionModes PositionMode = PositionModes.Point; 
		/// the texture to display as a gizmo when in position & texture mode
		[Tooltip("위치 및 텍스처 모드에 있을 때 기즈모로 표시할 텍스처")]
		[MMEnumCondition("PositionMode", (int)PositionModes.Texture)]
		public Texture PositionTexture; 
		/// the size of the texture to display as a gizmo
		[Tooltip("기즈모로 표시할 텍스처의 크기")]
		[MMEnumCondition("PositionMode", (int)PositionModes.Texture)]
		public Vector2 TextureSize = new Vector2(50f,50f); 
		/// the size of the gizmo when in position mode
		[Tooltip("위치 모드에 있을 때 기즈모의 크기")]
		[MMEnumCondition("GizmoType", (int)GizmoTypes.Position)]
		public float PositionSize = 0.2f; 
		/// whether to display the collider gizmo as a wire or a full mesh
		[Tooltip("콜라이더 기즈모를 와이어 또는 전체 메시로 표시할지 여부")]
		[MMEnumCondition("GizmoType", (int)GizmoTypes.Collider)]
		public ColliderRenderTypes ColliderRenderType = ColliderRenderTypes.Full;
		/// the distance from the scene view camera beyond which the gizmo won't be displayed
		[Tooltip("기즈모가 표시되지 않는 장면 뷰 카메라로부터의 거리")]
		public float ViewDistance = 20f; 
		
		[Header("Offsets")]
		/// an offset to apply when drawing a collider or position gizmo
		[Tooltip("충돌체 또는 위치 기즈모를 그릴 때 적용할 오프셋")]
		public Vector3 GizmoOffset = Vector3.zero;

		/// whether or not to lock the position of the gizmo on the x axis, regardless of the position of the object
		[Tooltip("객체의 위치에 관계없이 x축에서 기즈모의 위치를 ​​잠글지 여부")]
		public bool LockX = false;
		/// the position at which to put the gizmo when locked on the x axis
		[Tooltip("X축에 고정되었을 때 기즈모를 배치할 위치")]
		[MMCondition("LockX", true)]
		public float LockedX = 0f;
		
		/// whether or not to lock the position of the gizmo on the y axis, regardless of the position of the object
		[Tooltip("whether or not to lock the position of the gizmo on the y axis, regardless of the position of the object")]
		public bool LockY = false;
		/// the position at which to put the gizmo when locked on the y axis
		[Tooltip("the position at which to put the gizmo when locked on the y axis")]
		[MMCondition("LockY", true)]
		public float LockedY = 0f;
		
		/// whether or not to lock the position of the gizmo on the z axis, regardless of the position of the object
		[Tooltip("whether or not to lock the position of the gizmo on the z axis, regardless of the position of the object")]
		public bool LockZ = false;
		/// the position at which to put the gizmo when locked on the z axis
		[Tooltip("the position at which to put the gizmo when locked on the z axis")]
		[MMCondition("LockZ", true)]
		public float LockedZ = 0f;

		[Header("Text")]  
		/// whether or not to display text on that gizmo
		[Tooltip("해당 기즈모에 텍스트를 표시할지 여부")]
		public bool DisplayText = false; 
		/// what to display as text for that gizmo (some custom text, the object's name, position, rotation, scale, or a target property)
		[Tooltip("해당 기즈모에 대해 텍스트로 표시할 항목(일부 사용자 정의 텍스트, 개체 이름, 위치, 회전, 배율 또는 대상 속성)")]
		[MMCondition("DisplayText", true)]
		public TextModes TextMode; 
		/// when in CustomText mode, the text to display on that gizmo
		[Tooltip("CustomText 모드에 있을 때 해당 기즈모에 표시할 텍스트")]
		[MMEnumCondition("TextMode", (int)TextModes.CustomText)]
		public string TextToDisplay = "Some Text"; 
		/// the offset to apply to the text
		[Tooltip("텍스트에 적용할 오프셋")]
		[MMCondition("DisplayText", true)]
		public Vector3 TextOffset = new Vector3(0f, 0.5f, 0f);
		/// what style to use for the text's font
		[Tooltip("텍스트 글꼴에 사용할 스타일")]
		[MMCondition("DisplayText", true)]
		public FontStyle TextFontStyle = FontStyle.Normal; 
		/// the size of the text's font
		[Tooltip("텍스트 글꼴의 크기")]
		[MMCondition("DisplayText", true)]
		public int TextSize = 12; 
		/// the color in which to display the gizmo's text
		[Tooltip("기즈모의 텍스트를 표시할 색상")]
		[MMCondition("DisplayText", true)]
		public Color TextColor = MMColors.ReunoYellow; 
		/// the color of the background behind the text
		[Tooltip("텍스트 뒤의 배경색")]
		[MMCondition("DisplayText", true)]
		public Color TextBackgroundColor = new Color(0,0,0,0.3f); 
		/// the padding to apply to the text's background
		[Tooltip("텍스트의 배경에 적용할 패딩")]
		[MMCondition("DisplayText", true)]
		public Vector4 TextPadding = new Vector4(5,0,5,0); 
		/// the distance from the scene view camera beyond which the gizmo text won't be displayed
		[Tooltip("기즈모 텍스트가 표시되지 않는 씬 뷰 카메라로부터의 거리")]
		[MMCondition("DisplayText", true)]
		public float TextMaxDistance = 14f;
		/// when in Property mode, the property whose value to display on the gizmo
		[Tooltip("속성 모드에 있을 때 기즈모에 표시할 값의 속성")]
		public MMPropertyPicker TargetProperty;
		
		public bool Initialized { get; set; }
		public SphereCollider _sphereCollider { get; set; }
		public BoxCollider _boxCollider { get; set; }
		public MeshCollider _meshCollider { get; set; }
		public CircleCollider2D _circleCollider2D { get; set; }
		public BoxCollider2D _boxCollider2D { get; set; }
		public Vector3 _vector3Zero { get; set; }
		public Vector3 _newPosition { get; set; }
		public Vector2 _worldToGUIPosition { get; set; }
		public Rect _textureRect { get; set; }
		public GUIStyle _textGUIStyle { get; set; }
		public string _textToDisplay { get; set; }
		public bool _sphereColliderNotNull { get; set; }
		public bool _boxColliderNotNull { get; set; }
		public bool _meshColliderNotNull { get; set; }
		public bool _circleCollider2DNotNull { get; set; }
		public bool _boxCollider2DNotNull { get; set; }
		public bool _positionTextureNotNull { get; set; }
		
		#if UNITY_EDITOR
		
		/// <summary>
		/// On awake we initialize our property
		/// </summary>
		protected virtual void Awake()
		{
			TargetProperty.Initialization(this.gameObject);
		}
		
		#else 
		
		/// <summary>
		/// If we're not in editor, we disable ourselves
		/// </summary>
		protected virtual void Awake()
		{
			this.enabled = false;
		}
		
		#endif 
		
		
	}	
}