using UnityEngine;
using UnityEngine.InputSystem;

namespace ObstacleCourse
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        public GameController game;
        public SpriteRenderer visual;
        [Min(0.1f)] public float speed = 5f;
        [Min(0.1f)] public float protectionSeconds = 1.5f;
        public Vector2 spawn = new Vector2(-13, -4.5f);
        private Rigidbody2D body;
        private Vector2 movement;
        private float protectedUntil;
        private Color normalColor;
        public Vector2 Position => body.position;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            normalColor = visual.color;
        }

        private void Update()
        {
            movement = Vector2.zero;
            if (game.IsPlaying)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    movement.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                               - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                    movement.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                               - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
                }
                if (Gamepad.current != null)
                {
                    Vector2 stick = Gamepad.current.leftStick.ReadValue();
                    Vector2 pad = Gamepad.current.dpad.ReadValue();
                    if (stick.sqrMagnitude > 0.02f) movement = stick;
                    else if (pad.sqrMagnitude > 0) movement = pad;
                }
                // Impede que andar na diagonal seja mais rápido.
                movement = Vector2.ClampMagnitude(movement, 1);
            }
            bool blinking = game.IsPlaying && Time.time < protectedUntil;
            visual.color = blinking && Mathf.FloorToInt(Time.time * 10) % 2 == 0 ? Color.white : normalColor;
        }

        private void FixedUpdate()
        {
            body.linearVelocity = game.IsPlaying ? movement * speed : Vector2.zero;
            if (Mathf.Abs(body.position.x) > 16 || Mathf.Abs(body.position.y) > 8) ResetPlayer();
        }

        public void ResetPlayer()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            body.position = spawn;
            body.linearVelocity = Vector2.zero;
            movement = Vector2.zero;
            protectedUntil = 0;
            if (visual != null) visual.color = normalColor;
        }

        public void TouchHazard()
        {
            if (!game.IsPlaying || Time.time < protectedUntil) return;
            protectedUntil = Time.time + protectionSeconds;
            game.TakeDamage();
        }
    }
}
