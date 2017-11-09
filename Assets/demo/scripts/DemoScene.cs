using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Prime31;


public class DemoScene : MonoBehaviour
{
    // movement config
    public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;
    public float runMultiplier = 0.5f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
    private bool ledge;
    private Vector2 edgePos;

    private Transform grabGO;
    private bool crawling;

    void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController2D>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{

        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;

        if (_controller.collisionState.right && hit.transform.tag == "Edge")
        {
            ledge = true;
            transform.parent = hit.transform;
            transform.position = hit.transform.Find("Left").position;
            edgePos = hit.transform.Find("LeftHigh").position;
        }
        if (_controller.collisionState.left && hit.transform.tag == "Edge")
        {
            ledge = true;
            transform.parent = hit.transform;
            transform.position = hit.transform.Find("Right").position;
            edgePos = hit.transform.Find("RightHigh").position;
        }


        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
        
    }


    void onTriggerEnterEvent( Collider2D col )
	{
		Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider2D col )
	{
		Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		if( _controller.isGrounded )
			_velocity.y = 0;

        if(Input.GetAxis("Vertical")<=0)
            _controller.lookingUp = false;

        if (crawling)
        {
            GetComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.2f);
            GetComponent<BoxCollider2D>().offset = new Vector2(0f, -0.22f);
            _controller.recalculateDistanceBetweenRays();
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                crawling = false;
                GetComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.65f);
                GetComponent<BoxCollider2D>().offset = new Vector2(0f, 0f);
                _controller.recalculateDistanceBetweenRays();
            }
        }

        if (ledge)
        {
            normalizedHorizontalSpeed = 0f;
            _velocity = _controller.velocity = Vector3.zero ;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
                _animator.Play(Animator.StringToHash("Jump"));
                ledge = false;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                ledge = false;
                
                _animator.Play(Animator.StringToHash("Fall"));
            }
            else if(Input.GetAxis("Vertical") > 0)
            {
                _animator.Play(Animator.StringToHash("Ledge_Hang"));

                transform.position = Vector2.Lerp(transform.position, edgePos, Time.deltaTime* _controller.legdeSpeed); 
                if (Vector2.Distance(transform.position, edgePos) < 0.1f)
                    ledge = false;
            }
            else _animator.Play(Animator.StringToHash("Ledge"));

            if(!ledge) transform.parent = null;

            return;
        }

        if(Input.GetKey(KeyCode.C) && _controller.isGrounded)
        {

            var initialRayOrigin = (Mathf.Sign(transform.localScale.x)==1f) ? _controller._raycastOrigins.bottomRight : _controller._raycastOrigins.bottomLeft;
            var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y);

            RaycastHit2D _charRaycasthit2D = Physics2D.Raycast(ray, Mathf.Sign(transform.localScale.x) * Vector2.right, _controller.skinWidth, _controller.platformMask);
            Debug.Log(_charRaycasthit2D.transform);
            if(_charRaycasthit2D.transform && _charRaycasthit2D.transform.tag == "Grab")
            {
                _charRaycasthit2D.transform.parent = transform;
                _charRaycasthit2D.transform.GetComponent<Rigidbody2D>().simulated = false;
                grabGO = _charRaycasthit2D.transform;
            }
        }
        if(Input.GetKeyUp(KeyCode.C) && grabGO)
        {
            grabGO.GetComponent<Rigidbody2D>().simulated = true;
            grabGO.parent = null;
            grabGO = null;
        }

        if ( Input.GetAxis("Vertical")>0 && Mathf.Abs(_velocity.x) <= 0.1f)
        {
            normalizedHorizontalSpeed = 0f;

            _controller.lookingUp = true;

            if (_controller.isGrounded)
                _animator.Play(Animator.StringToHash("LookingUp"));
        }
        else if( Input.GetKey( KeyCode.RightArrow ) )
		{
			normalizedHorizontalSpeed = 1;
           
            if (Input.GetKey(KeyCode.LeftControl) && _controller.isGrounded) normalizedHorizontalSpeed = 0.5f;
            else if (Input.GetKey(KeyCode.LeftShift)) normalizedHorizontalSpeed = 1 + normalizedHorizontalSpeed * runMultiplier;

            if ( transform.localScale.x < 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

            if (_controller.isGrounded && Input.GetKey(KeyCode.LeftControl)) { 
                _animator.Play(Animator.StringToHash("Crawl"));
                crawling = true;
            }
            else if (_controller.isGrounded)
                _animator.Play( Animator.StringToHash( "Run" ) );
		}
		else if( Input.GetKey( KeyCode.LeftArrow ) )
		{
			normalizedHorizontalSpeed = -1;
            if (Input.GetKey(KeyCode.LeftControl) && _controller.isGrounded) normalizedHorizontalSpeed = -0.5f;
            else if (Input.GetKey(KeyCode.LeftShift)) normalizedHorizontalSpeed = -(1 - normalizedHorizontalSpeed * runMultiplier);

            if ( transform.localScale.x > 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

            if (_controller.isGrounded && Input.GetKey(KeyCode.LeftControl))
            {
                _animator.Play(Animator.StringToHash("Crawl"));
                crawling = true;
            }
            else if (_controller.isGrounded)
                _animator.Play(Animator.StringToHash("Run"));
		}
		else
		{
			normalizedHorizontalSpeed = 0;

            if (_controller.isGrounded && Input.GetKey(KeyCode.LeftControl))
            {
                _animator.Play(Animator.StringToHash("Duck"));
                crawling = true;
            }
            else if ( _controller.isGrounded )
				_animator.Play( Animator.StringToHash( "Idle" ) );
		}


		// we can only jump whilst grounded
		if( _controller.isGrounded && Input.GetKeyDown( KeyCode.Space ) && Input.GetAxis("Vertical")>=0 )
		{
			_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
			_animator.Play( Animator.StringToHash( "Jump" ) );
		}


		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor );

		// apply gravity before moving
		_velocity.y += gravity * Time.deltaTime;

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets us jump down through one way platforms
		if( _controller.isGrounded && Input.GetKey( KeyCode.DownArrow ) && Input.GetKeyDown(KeyCode.Space))
		{
			_velocity.y *= 3f;
			_controller.ignoreOneWayPlatformsThisFrame = true;
            _animator.Play(Animator.StringToHash("Fall"));
        }

		_controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
	}

}
