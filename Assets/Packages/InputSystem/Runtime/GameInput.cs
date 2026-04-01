namespace Input
{
	/// <summary>
	/// A script that contains every single <see cref="InputButton"/> that the game has available. <para/>
	/// <b>WARNING:</b> Do <b>NOT</b> modify this script unless you know what you're doing. <br/>
	/// This script is automatically generated in the Editor whenever the Input Action Asset is changed.
	/// This means that no manual editing is required as every single InputAction is automatically added to this script.
	/// </summary>
	public static class GameInput
	{
		/// <summary>
		/// The current <see cref="ControlScheme"/> the player is using, courtesy of the <see cref="ControlSchemeManager"/>.
		/// </summary>
		public static ControlScheme CurrentControlScheme => GameInputManager.CurrentControlScheme;

        // Generation Begin
		public static Vector2Button Move { get; private set; }
		public static Vector2Button Look { get; private set; }
		public static PressButton Attack { get; private set; }
		public static PressButton Interact { get; private set; }
		public static PressButton Crouch { get; private set; }
		public static PressButton Jump { get; private set; }
		public static PressButton Previous { get; private set; }
		public static PressButton Next { get; private set; }
		public static PressButton Sprint { get; private set; }

		public static class UI
		{
			public static Vector2Button Navigate { get; private set; }
			public static PressButton Submit { get; private set; }
			public static PressButton Cancel { get; private set; }
			public static Vector2Button Point { get; private set; }
			public static PressButton Click { get; private set; }
			public static PressButton RightClick { get; private set; }
			public static PressButton MiddleClick { get; private set; }
			public static Vector2Button ScrollWheel { get; private set; }
			public static PressButton TrackedDevicePosition { get; private set; }
			public static PressButton TrackedDeviceOrientation { get; private set; }
		}
		// Generation End
	}
}