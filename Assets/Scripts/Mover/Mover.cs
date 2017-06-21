using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//a PhysicsMover is just like a Mover except it uses Unity's built-in physics to move
public class Mover : MonoBehaviour
{
    [Tooltip ("How much we accelerate over time on the ground. the higher it is, the fast we gain speed.")]
    public float acceleration = 50.0f;

    [Tooltip ("How much we accelerate when we're in the air.")]
    public float aerialAcceleration = 10.0f;
    
    [Tooltip ("How much force we apply when we jump into the air. the higher it is, the higher we jump.")]
    public float jumpImpulse = 10.0f;
    
    [Tooltip ("How fast we can go at the very maximum.")]
    public float maximumSpeed = 20.0f;

    [Tooltip ("When our X velocity is lower than this, we are standing. This is mostly for aesthetic reasons like animations.")]
    public float minimumWalkSpeed = 0.1f;


	private List<GameObject> collisionList;
    
    //this is Mover has an Animator attached, we can play animations as we go.
    private Animator animator;

    //we need to initialize isOnGround to be false, since we start in the air.
    public void Start()
    {
		collisionList = new List<GameObject>();
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        //if we have an Animator, tell it how to animate
        if ( animator != null )
        {
            //tell the animator if we're walking or not
            animator.SetBool( "walking", IsWalking() );
        }
    }

	public bool isOnGround()
	{
		if (collisionList.Count > 0) {
			return true;
		}
		return false;
	}
    
    //this tells our Rigidbody to accelerate in a given direction, using our acceleration or
    //aerialAcceleration values, depending on if we're in the air or not.
    public void AccelerateInDirection(Vector2 direction)
    {
        float accel = acceleration;
		if ( !isOnGround() )
        {
            accel = aerialAcceleration;
        }

        //GetComponent<type>() will give you the component of the given type that is attached to this same object
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector3 newVelocity = rb.velocity + direction * accel * Time.deltaTime;
        newVelocity.x = Mathf.Clamp( newVelocity.x, -maximumSpeed, maximumSpeed );
        rb.velocity = newVelocity;
    }
    
    //applies a single burst of velocity upwards - jump!
    public void Jump()
    {
        //only apply the velocity if we're currently standing on the ground
		if ( isOnGround() )
        {
            GetComponent<Rigidbody2D>().velocity += new Vector2( 0.0f, jumpImpulse );
    
			collisionList.Clear ();
            //tell our animator to play a jump animation
            if ( animator != null )
            {
                animator.SetBool( "jumping", true );
            }
        }
    }
    
    //Unity will automatically call this on a MonoBehaviour on the frame that a collision starts
    //between 2 colliders. note that occasionally this doesn't get called - thanks Unity!
    public void OnCollisionEnter2D(Collision2D collision)
    {
        //we collided, so that means we're on the ground
        //note that this is a pretty poor way of doing it, since if we hit our head it would also mean we had
        //jumped... maybe you'd fix that by checking collision.normal?
        //this also doesn't work well when we collide with multiple objects, and can fully break.
        //how might we handle multiple collisions? Maybe a list?
        if ( collision.collider.gameObject.layer == 8 )
        {
			if (!collisionList.Contains(collision.collider.gameObject)) {
				collisionList.Add (collision.collider.gameObject);
			}

            //tell our animator to play a jump animation
            if ( animator != null )
            {
                animator.SetBool( "jumping", false );
            }
        }
    }
    
    //Unity will automatically call this on a MonoBehaviour on the frame that a collision ends
    public void OnCollisionExit2D(Collision2D collision)
    {
        //we're not colliding anymore, so we're no longer standing on the ground
        if ( collision.collider.gameObject.layer == 8 )
        {
			collisionList.Remove (collision.collider.gameObject);

            //tell our animator to play a jump animation
            if ( animator != null )
            {
                animator.SetBool( "jumping", true );
            }
        }
    }

    //this is convenient for controllers to know. we're walking if we have any x velocity.
    public bool IsWalking()
    {
        return Mathf.Abs( GetComponent<Rigidbody2D>().velocity.x ) >= minimumWalkSpeed;
    }
}