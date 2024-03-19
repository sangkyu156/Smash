using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 영구 싱글톤은 입력을 처리하고 플레이어에 명령을 보냅니다.
    /// 중요: 이 스크립트의 실행 순서는 -100이어야 합니다.
    /// 스크립트 파일을 클릭한 다음 스크립트 검사기 오른쪽 하단에 있는 실행 순서 버튼을 클릭하여 스크립트의 실행 순서를 정의할 수 있습니다.
    /// 자세한 내용은 https://docs.unity3d.com/Manual/class-ScriptExecution.html을 참조하세요.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Input Manager")]
    public class InputManager : MMSingleton<InputManager> /*MMEventListener<MMInventoryEvent>*/
    {
        [Header("Settings")]
        /// set this to false to prevent the InputManager from reading input
        [Tooltip("InputManager가 입력을 읽지 못하도록 하려면 false로 설정하세요.")]
        public bool InputDetectionActive = true;
        /// if this is true, button states will be reset on focus loss - when clicking outside the player window on PC, for example
        [Tooltip("이것이 사실이라면 버튼 상태는 포커스 손실 시 재설정됩니다. 예를 들어 PC에서 플레이어 창 외부를 클릭하면")]
        public bool ResetButtonStatesOnFocusLoss = true;

        [Header("Player binding")]
        [MMInformation("InputManager에서 가장 먼저 설정해야 할 것은 PlayerID입니다. 이 ID는 입력 관리자를 캐릭터에 바인딩하는 데 사용됩니다. Player1, Player2, Player3 또는 Player4와 함께 가고 싶을 것입니다.", MMInformationAttribute.InformationType.Info, false)]
        /// a string identifying the target player(s). You'll need to set this exact same string on your Character, and set its type to Player
        [Tooltip("대상 플레이어를 식별하는 문자열입니다. 캐릭터에 정확히 동일한 문자열을 설정하고 해당 유형을 플레이어로 설정해야 합니다.")]
        public string PlayerID = "Player1";
        /// the possible modes for this input manager
        public enum InputForcedModes { None, Mobile, Desktop }
        /// the possible kinds of control used for movement
        public enum MovementControls { Joystick, Arrows }

        [Header("Mobile controls")]
        [MMInformation("자동 모바일 감지를 선택하면 빌드 타겟이 Android 또는 iOS일 때 엔진이 자동으로 모바일 컨트롤로 전환됩니다. 아래 드롭다운을 사용하여 모바일 또는 데스크톱(키보드, 게임 패드) 컨트롤을 강제할 수도 있습니다.\n모바일 컨트롤 및/또는 GUI가 필요하지 않은 경우 이 구성 요소는 자체적으로 작동할 수도 있습니다. 대신 빈 게임 개체에 넣기만 하면 됩니다.", MMInformationAttribute.InformationType.Info, false)]
        /// if this is set to true, the InputManager will try to detect what mode it should be in, based on the current target device
        [Tooltip("이것이 true로 설정되면, InputManager는 현재 대상 장치를 기반으로 어떤 모드에 있어야 하는지 감지하려고 시도합니다.")]
        public bool AutoMobileDetection = true;
        /// use this to force desktop (keyboard, pad) or mobile (touch) mode
        [Tooltip("데스크탑(키보드, 패드) 또는 모바일(터치) 모드를 강제하려면 이 옵션을 사용하십시오.")]
        public InputForcedModes InputForcedMode;
        /// if this is true, the weapon mode will be forced to the selected WeaponForcedMode
        [Tooltip("이것이 사실이라면 무기 모드는 선택된 WeaponForcedMode로 강제됩니다.")]
        public bool ForceWeaponMode = false;
        /// use this to force a control mode for weapons
        [MMCondition("ForceWeaponMode", true)]
        [Tooltip("이것을 사용하여 무기 제어 모드를 강제 실행합니다.")]
        public WeaponAim.AimControls WeaponForcedMode;
        /// if this is true, mobile controls will be hidden in editor mode, regardless of the current build target or the forced mode
        [Tooltip("이것이 사실이라면 현재 빌드 대상이나 강제 모드에 관계없이 모바일 컨트롤이 편집기 모드에서 숨겨집니다.")]
        public bool HideMobileControlsInEditor = false;
        /// use this to specify whether you want to use the default joystick or arrows to move your character
        [Tooltip("캐릭터를 움직일 때 기본 조이스틱이나 화살표를 사용할지 여부를 지정하려면 이 옵션을 사용하세요.")]
        public MovementControls MovementControl = MovementControls.Joystick;
        /// if this is true, we're currently in mobile mode
        public bool IsMobile { get; protected set; }

        [Header("Movement settings")]
        [MMInformation("컨트롤에 관성을 적용하려면 SmoothMovement를 켜십시오. 즉, 방향을 누르거나 떼는 것과 캐릭터가 움직이거나 멈추는 사이에 약간의 지연이 있음을 의미합니다. 여기에서 수평 및 수직 임계값을 정의할 수도 있습니다.", MMInformationAttribute.InformationType.Info, false)]
        /// If set to true, acceleration / deceleration will take place when moving / stopping
        [Tooltip("true로 설정하면 이동/정지 시 가속/감속이 발생합니다.")]
        public bool SmoothMovement = true;
        /// the minimum horizontal and vertical value you need to reach to trigger movement on an analog controller (joystick for example)
        [Tooltip("아날로그 컨트롤러(예: 조이스틱)에서 움직임을 트리거하기 위해 도달해야 하는 최소 수평 및 수직 값")]
        public Vector2 Threshold = new Vector2(0.1f, 0.4f);

        [Header("Camera Rotation")]
        [MMInformation("여기에서 카메라 회전이 입력에 영향을 미칠지 여부를 결정할 수 있습니다. 예를 들어 3D 아이소메트릭 게임에서 '위'가 Vector3.up/forward가 아닌 다른 방향을 의미하도록 하려는 경우 유용할 수 있습니다.", MMInformationAttribute.InformationType.Info, false)]
        /// if this is true, any directional input coming into this input manager will be rotated to align with the current camera orientation
        [Tooltip("이것이 사실이라면 이 입력 관리자로 들어오는 모든 방향 입력은 현재 카메라 방향에 맞춰 회전됩니다.")]
        public bool RotateInputBasedOnCameraDirection = false;

        /// the jump button, used for jumps and validation
        public MMInput.IMButton JumpButton { get; protected set; }
        /// the run button
        public MMInput.IMButton RunButton { get; protected set; }
        /// the dash button
        public MMInput.IMButton DashButton { get; protected set; }
        /// the 웅크림 button
        public MMInput.IMButton CrouchButton { get; protected set; }
        /// the shoot button
        public MMInput.IMButton ShootButton { get; protected set; }
        /// 영역과의 상호 작용에 사용되는 활성화 버튼
        public MMInput.IMButton InteractButton { get; protected set; }
        /// the shoot button
        public MMInput.IMButton SecondaryShootButton { get; protected set; }
        /// the reload button
        public MMInput.IMButton ReloadButton { get; protected set; }
        /// the pause button
        public MMInput.IMButton PauseButton { get; protected set; }
        /// the time control button
        public MMInput.IMButton TimeControlButton { get; protected set; }
        /// the button used to switch character (either via model or prefab switch)
        public MMInput.IMButton SwitchCharacterButton { get; protected set; }
        /// the switch weapon button
        public MMInput.IMButton SwitchWeaponButton { get; protected set; }
        /// the shoot axis, used as a button (non analogic)
        public MMInput.ButtonStates ShootAxis { get; protected set; }
        /// the shoot axis, used as a button (non analogic)
        public MMInput.ButtonStates SecondaryShootAxis { get; protected set; }
        /// the primary movement value (used to move the character around)
        public Vector2 PrimaryMovement { get { return _primaryMovement; } }
        /// the secondary movement (usually the right stick on a gamepad), used to aim
        public Vector2 SecondaryMovement { get { return _secondaryMovement; } }
        /// the primary movement value (used to move the character around)
        public Vector2 LastNonNullPrimaryMovement { get; set; }
        /// the secondary movement (usually the right stick on a gamepad), used to aim
        public Vector2 LastNonNullSecondaryMovement { get; set; }
        /// the camera rotation axis input value
        public float CameraRotationInput { get { return _cameraRotationInput; } }
        /// the current camera angle
        public float CameraAngle { get { return _cameraAngle; } }
        /// the position of the mouse
        public virtual Vector2 MousePosition => Input.mousePosition;

        protected Camera _targetCamera;
        protected bool _camera3D;
        protected float _cameraAngle;
        protected List<MMInput.IMButton> ButtonList;
        protected Vector2 _primaryMovement = Vector2.zero;
        protected Vector2 _secondaryMovement = Vector2.zero;
        protected float _cameraRotationInput = 0f;
        protected string _axisHorizontal;
        protected string _axisVertical;
        protected string _axisSecondaryHorizontal;
        protected string _axisSecondaryVertical;
        protected string _axisShoot;
        protected string _axisShootSecondary;
        protected string _axisCamera;

        /// <summary>
        /// On Awake we run our pre-initialization
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            PreInitialization();
        }

        /// <summary>
        /// On Start we look for what mode to use, and initialize our axis and buttons
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
        }

        /// <summary>
        /// Initializes buttons and axis
        /// </summary>
        protected virtual void PreInitialization()
        {
            InitializeButtons();
            InitializeAxis();
        }

        /// <summary>
        /// On init we auto detect control schemes
        /// </summary>
        protected virtual void Initialization()
        {
            ControlsModeDetection();
        }

        /// <summary>
        /// Turns mobile controls on or off depending on what's been defined in the inspector, and what target device we're on
        /// </summary>
        public virtual void ControlsModeDetection()
        {
            if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(false); }
            IsMobile = false;
            if (AutoMobileDetection)
            {
#if UNITY_ANDROID || UNITY_IPHONE
					if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(true,MovementControl); }
					IsMobile = true;
#endif
            }
            if (InputForcedMode == InputForcedModes.Mobile)
            {
                if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(true, MovementControl); }
                IsMobile = true;
            }
            if (InputForcedMode == InputForcedModes.Desktop)
            {
                if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(false); }
                IsMobile = false;
            }
            if (HideMobileControlsInEditor)
            {
#if UNITY_EDITOR
                if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(false); }
                IsMobile = false;
#endif
            }
        }

        /// <summary>
        /// 버튼을 초기화합니다. 더 많은 버튼을 추가하려면 여기에 등록하세요.
        /// </summary>
        protected virtual void InitializeButtons()
        {
            ButtonList = new List<MMInput.IMButton>();
            ButtonList.Add(JumpButton = new MMInput.IMButton(PlayerID, "Jump", JumpButtonDown, JumpButtonPressed, JumpButtonUp));
            ButtonList.Add(RunButton = new MMInput.IMButton(PlayerID, "Run", RunButtonDown, RunButtonPressed, RunButtonUp));
            ButtonList.Add(InteractButton = new MMInput.IMButton(PlayerID, "Interact", InteractButtonDown, InteractButtonPressed, InteractButtonUp));
            ButtonList.Add(DashButton = new MMInput.IMButton(PlayerID, "Dash", DashButtonDown, DashButtonPressed, DashButtonUp));
            ButtonList.Add(CrouchButton = new MMInput.IMButton(PlayerID, "Crouch", CrouchButtonDown, CrouchButtonPressed, CrouchButtonUp));
            ButtonList.Add(SecondaryShootButton = new MMInput.IMButton(PlayerID, "SecondaryShoot", SecondaryShootButtonDown, SecondaryShootButtonPressed, SecondaryShootButtonUp));
            ButtonList.Add(ShootButton = new MMInput.IMButton(PlayerID, "Shoot", ShootButtonDown, ShootButtonPressed, ShootButtonUp));
            ButtonList.Add(ReloadButton = new MMInput.IMButton(PlayerID, "Reload", ReloadButtonDown, ReloadButtonPressed, ReloadButtonUp));
            ButtonList.Add(SwitchWeaponButton = new MMInput.IMButton(PlayerID, "SwitchWeapon", SwitchWeaponButtonDown, SwitchWeaponButtonPressed, SwitchWeaponButtonUp));
            ButtonList.Add(PauseButton = new MMInput.IMButton(PlayerID, "Pause", PauseButtonDown, PauseButtonPressed, PauseButtonUp));
            ButtonList.Add(TimeControlButton = new MMInput.IMButton(PlayerID, "TimeControl", TimeControlButtonDown, TimeControlButtonPressed, TimeControlButtonUp));
            ButtonList.Add(SwitchCharacterButton = new MMInput.IMButton(PlayerID, "SwitchCharacter", SwitchCharacterButtonDown, SwitchCharacterButtonPressed, SwitchCharacterButtonUp));
        }

        /// <summary>
        /// Initializes the axis strings.
        /// </summary>
        protected virtual void InitializeAxis()
        {
            _axisHorizontal = PlayerID + "_Horizontal";
            _axisVertical = PlayerID + "_Vertical";
            _axisSecondaryHorizontal = PlayerID + "_SecondaryHorizontal";
            _axisSecondaryVertical = PlayerID + "_SecondaryVertical";
            _axisShoot = PlayerID + "_ShootAxis";
            _axisShootSecondary = PlayerID + "_SecondaryShootAxis";
            _axisCamera = PlayerID + "_CameraRotationAxis";
        }

        /// <summary>
        /// On LateUpdate, we process our button states
        /// </summary>
        protected virtual void LateUpdate()
        {
            ProcessButtonStates();
        }

        /// <summary>
        /// 업데이트 시 다양한 명령을 확인하고 그에 따라 값과 상태를 업데이트합니다.
        /// </summary>
        protected virtual void Update()
        {
            if (!IsMobile && InputDetectionActive)
            {
                SetMovement();
                SetSecondaryMovement();
                SetShootAxis();
                SetCameraRotationAxis();
                GetInputButtons();
                GetLastNonNullValues();
            }
        }

        /// <summary>
        /// Gets the last non null values for both primary and secondary axis
        /// </summary>
        protected virtual void GetLastNonNullValues()
        {
            if (_primaryMovement.magnitude > Threshold.x)
            {
                LastNonNullPrimaryMovement = _primaryMovement;
            }
            if (_secondaryMovement.magnitude > Threshold.x)
            {
                LastNonNullSecondaryMovement = _secondaryMovement;
            }
        }

        /// <summary>
        /// 모바일을 사용하지 않는 경우 입력 변경을 관찰하고 그에 따라 버튼 상태를 업데이트합니다.
        /// </summary>
        protected virtual void GetInputButtons()
        {
            foreach (MMInput.IMButton button in ButtonList)
            {
                if (Input.GetButton(button.ButtonID))
                {
                    button.TriggerButtonPressed();
                }
                if (Input.GetButtonDown(button.ButtonID))
                {
                    button.TriggerButtonDown();
                }
                if (Input.GetButtonUp(button.ButtonID))
                {
                    button.TriggerButtonUp();
                }
            }
        }

        /// <summary>
        /// Called at LateUpdate(), this method processes the button states of all registered buttons
        /// </summary>
        public virtual void ProcessButtonStates()
        {
            // for each button, if we were at ButtonDown this frame, we go to ButtonPressed. If we were at ButtonUp, we're now Off
            foreach (MMInput.IMButton button in ButtonList)
            {
                if (button.State.CurrentState == MMInput.ButtonStates.ButtonDown)
                {
                    button.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
                }
                if (button.State.CurrentState == MMInput.ButtonStates.ButtonUp)
                {
                    button.State.ChangeState(MMInput.ButtonStates.Off);
                }
            }
        }

        /// <summary>
        /// Called every frame, if not on mobile, gets primary movement values from input
        /// </summary>
        public virtual void SetMovement()
        {
            if (!IsMobile && InputDetectionActive)
            {
                if (SmoothMovement)
                {
                    _primaryMovement.x = Input.GetAxis(_axisHorizontal);
                    _primaryMovement.y = Input.GetAxis(_axisVertical);
                }
                else
                {
                    _primaryMovement.x = Input.GetAxisRaw(_axisHorizontal);
                    _primaryMovement.y = Input.GetAxisRaw(_axisVertical);
                }
                _primaryMovement = ApplyCameraRotation(_primaryMovement);
            }
        }

        /// <summary>
        /// Called every frame, if not on mobile, gets secondary movement values from input
        /// </summary>
        public virtual void SetSecondaryMovement()
        {
            if (!IsMobile && InputDetectionActive)
            {
                if (SmoothMovement)
                {
                    _secondaryMovement.x = Input.GetAxis(_axisSecondaryHorizontal);
                    _secondaryMovement.y = Input.GetAxis(_axisSecondaryVertical);
                }
                else
                {
                    _secondaryMovement.x = Input.GetAxisRaw(_axisSecondaryHorizontal);
                    _secondaryMovement.y = Input.GetAxisRaw(_axisSecondaryVertical);
                }
                _secondaryMovement = ApplyCameraRotation(_secondaryMovement);
            }
        }

        /// <summary>
        /// Called every frame, if not on mobile, gets shoot axis values from input
        /// </summary>
        protected virtual void SetShootAxis()
        {
            if (!IsMobile && InputDetectionActive)
            {
                ShootAxis = MMInput.ProcessAxisAsButton(_axisShoot, Threshold.y, ShootAxis);
                SecondaryShootAxis = MMInput.ProcessAxisAsButton(_axisShootSecondary, Threshold.y, SecondaryShootAxis, MMInput.AxisTypes.Positive);
            }
        }

        /// <summary>
        /// Grabs camera rotation input and stores it
        /// </summary>
        protected virtual void SetCameraRotationAxis()
        {
            if (!IsMobile)
            {
                _cameraRotationInput = Input.GetAxis(_axisCamera);
            }
        }

        /// <summary>
        /// If you're using a touch joystick, bind your main joystick to this method
        /// </summary>
        /// <param name="movement">Movement.</param>
        public virtual void SetMovement(Vector2 movement)
        {
            if (IsMobile && InputDetectionActive)
            {
                _primaryMovement.x = movement.x;
                _primaryMovement.y = movement.y;
            }
            _primaryMovement = ApplyCameraRotation(_primaryMovement);
        }

        /// <summary>
        /// If you're using a touch joystick, bind your secondary joystick to this method
        /// </summary>
        /// <param name="movement">Movement.</param>
        public virtual void SetSecondaryMovement(Vector2 movement)
        {
            if (IsMobile && InputDetectionActive)
            {
                _secondaryMovement.x = movement.x;
                _secondaryMovement.y = movement.y;
            }
            _secondaryMovement = ApplyCameraRotation(_secondaryMovement);
        }

        /// <summary>
        /// If you're using touch arrows, bind your left/right arrows to this method
        /// </summary>
        /// <param name="">.</param>
        public virtual void SetHorizontalMovement(float horizontalInput)
        {
            if (IsMobile && InputDetectionActive)
            {
                _primaryMovement.x = horizontalInput;
            }
        }

        /// <summary>
        /// If you're using touch arrows, bind your secondary down/up arrows to this method
        /// </summary>
        /// <param name="">.</param>
        public virtual void SetVerticalMovement(float verticalInput)
        {
            if (IsMobile && InputDetectionActive)
            {
                _primaryMovement.y = verticalInput;
            }
        }

        /// <summary>
        /// If you're using touch arrows, bind your secondary left/right arrows to this method
        /// </summary>
        /// <param name="">.</param>
        public virtual void SetSecondaryHorizontalMovement(float horizontalInput)
        {
            if (IsMobile && InputDetectionActive)
            {
                _secondaryMovement.x = horizontalInput;
            }
        }

        /// <summary>
        /// If you're using touch arrows, bind your down/up arrows to this method
        /// </summary>
        /// <param name="">.</param>
        public virtual void SetSecondaryVerticalMovement(float verticalInput)
        {
            if (IsMobile && InputDetectionActive)
            {
                _secondaryMovement.y = verticalInput;
            }
        }

        /// <summary>
        /// Sets an associated camera, used to rotate input based on camera position
        /// </summary>
        /// <param name="targetCamera"></param>
        /// <param name="rotationAxis"></param>
        public virtual void SetCamera(Camera targetCamera, bool camera3D)
        {
            _targetCamera = targetCamera;
            _camera3D = camera3D;
        }

        /// <summary>
        /// Sets the current camera rotation input, which you'll want to keep between -1 (left) and 1 (right), 0 being no rotation
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetCameraRotationInput(float newValue)
        {
            _cameraRotationInput = newValue;
        }

        /// <summary>
        /// Rotates input based on camera orientation
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual Vector2 ApplyCameraRotation(Vector2 input)
        {
            if (!InputDetectionActive)
            {
                return Vector2.zero;
            }

            if (RotateInputBasedOnCameraDirection)
            {
                if (_camera3D)
                {
                    _cameraAngle = _targetCamera.transform.localEulerAngles.y;
                    return MMMaths.RotateVector2(input, -_cameraAngle);
                }
                else
                {
                    _cameraAngle = _targetCamera.transform.localEulerAngles.z;
                    return MMMaths.RotateVector2(input, _cameraAngle);
                }
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// If we lose focus, we reset the states of all buttons
        /// </summary>
        /// <param name="hasFocus"></param>
        protected void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && ResetButtonStatesOnFocusLoss && (ButtonList != null))
            {
                ForceAllButtonStatesTo(MMInput.ButtonStates.ButtonUp);
            }
        }

        /// <summary>
        /// Lets you force the state of all buttons in the InputManager to the one specified in parameters
        /// </summary>
        /// <param name="newState"></param>
        public virtual void ForceAllButtonStatesTo(MMInput.ButtonStates newState)
        {
            foreach (MMInput.IMButton button in ButtonList)
            {
                button.State.ChangeState(newState);
            }
        }

        public virtual void JumpButtonDown() { JumpButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void JumpButtonPressed() { JumpButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void JumpButtonUp() { JumpButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void DashButtonDown() { DashButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void DashButtonPressed() { DashButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void DashButtonUp() { DashButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void CrouchButtonDown() { CrouchButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void CrouchButtonPressed() { CrouchButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void CrouchButtonUp() { CrouchButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void RunButtonDown() { RunButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void RunButtonPressed() { RunButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void RunButtonUp() { RunButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void ReloadButtonDown() { ReloadButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void ReloadButtonPressed() { ReloadButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void ReloadButtonUp() { ReloadButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void InteractButtonDown() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void InteractButtonPressed() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void InteractButtonUp() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void ShootButtonDown() { ShootButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void ShootButtonPressed() { ShootButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void ShootButtonUp() { ShootButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void SecondaryShootButtonDown() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SecondaryShootButtonPressed() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SecondaryShootButtonUp() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void PauseButtonDown() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void PauseButtonPressed() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void PauseButtonUp() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void TimeControlButtonDown() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void TimeControlButtonPressed() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void TimeControlButtonUp() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void SwitchWeaponButtonDown() { SwitchWeaponButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SwitchWeaponButtonPressed() { SwitchWeaponButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SwitchWeaponButtonUp() { SwitchWeaponButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void SwitchCharacterButtonDown() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SwitchCharacterButtonPressed() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SwitchCharacterButtonUp() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }
    }
}