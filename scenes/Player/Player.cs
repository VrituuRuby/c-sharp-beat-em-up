using Godot;

public partial class Player : CharacterBody2D
{
	public enum PlayerState { Idle, Walk, Attack }

	public PlayerState state = PlayerState.Idle;
	public enum AttackMoves { Punch, LowKick, HighKick }
	public AttackMoves currentAttack = AttackMoves.Punch;
	private Sprite2D _playerSprites;
	private AnimationPlayer _animationPlayer;
	private Area2D _hitBoxArea;
	private Label label;
	private Timer _attackCoolDown;

	private Vector2 _motion = Vector2.Zero;
	private Vector2 _velocity = Vector2.Zero;
	private int _speed = 80;
	private float _friction = 0.2f;

	public override void _Ready()
	{
		_playerSprites = GetNode<Sprite2D>("Sprites");
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_hitBoxArea = GetNode<Area2D>("HitBox");
		label = GetNode<Label>("Label");
		_attackCoolDown = GetNode<Timer>("AttackCooldown");
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleInput(delta);

		label.Text = state.ToString();

		switch (state)
		{
			case PlayerState.Idle:
				Idle(delta);
				break;
			case PlayerState.Walk:
				Walk(delta);
				break;
			case PlayerState.Attack:
				Attack(delta);
				break;
		}

		bool _flipH = _motion.X == 0 ? _playerSprites.FlipH : _motion.X > 0 ? false : true;
		_playerSprites.FlipH = _flipH;
		_hitBoxArea.Scale = _hitBoxArea.Scale with { X = _flipH ? -1 : 1 };

		_velocity.X = Mathf.Lerp(_velocity.X, _motion.X * _speed * (float)delta, _friction);
		_velocity.Y = Mathf.Lerp(_velocity.Y, _motion.Y * _speed * (float)delta, _friction);

		MoveAndCollide(_velocity);
	}

	private void Idle(double delta)
	{
		_motion = Vector2.Zero;
		_animationPlayer.Play("idle");
	}

	private void Walk(double delta)
	{
		_animationPlayer.Play("walk");
	}

	private void Attack(double delta)
	{
		_attackCoolDown.Start();

		int direction = _playerSprites.FlipH ? -1 : 1;
		_velocity.X = Mathf.Lerp(_velocity.X, 10 * direction, _friction) * (float)delta;
		_velocity.Y = Mathf.Lerp(_velocity.Y, 0, _friction);
		switch (currentAttack)
		{
			case AttackMoves.Punch:
				_animationPlayer.Play("punch_animation");
				break;
			case AttackMoves.LowKick:
				_animationPlayer.Play("low_kick");
				break;
			case AttackMoves.HighKick:
				_animationPlayer.Play("high_kick");
				break;
		}

	}

	private void _OnAttackCoolDownTimeout()
	{
		currentAttack = AttackMoves.Punch;
	}

	public void _OnAnimationFinished(string AnimName)
	{
		GD.Print(AnimName);
		currentAttack = (AttackMoves)(((int)currentAttack + 1) % 3);
		state = PlayerState.Idle;
	}

	private void HandleInput(double delta)
	{
		_motion.X = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		_motion.Y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");

		_motion = _motion.Normalized();

		if (Input.IsActionJustPressed("attack"))
		{
			state = PlayerState.Attack;
			return;
		}

		if (state == PlayerState.Attack) return;

		if (_motion.Length() > 0)
		{
			state = PlayerState.Walk;
			return;
		}

		state = PlayerState.Idle;
	}

}
