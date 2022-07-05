using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehicleController : MonoBehaviour,IHonkable, IPoolObject
{
    public enum ControlType { playerControlled,AI_FreeRoam,AI_FollowPath,Parked}
    public ControlType controlType;
    float desiredVel = 20;
    float damping = 0;

    [Header("Parameters")]
    [Range(1,120)]
    public float maxSpeed = 50;
    [Range(0.2f, 5f)]
    public float acceleration = 2f;
    [Range(0,1)]
    public float brakefactor = 0.5f;
    [Range(0,75)]
    public float maxSteer = 30f;
    [Range(0,5)]
    public float tireGrip = 0.8f;
    [Range(0,1)]
    public float driftGripReduction = 0.5f;
    [Range(0.001f,2f)]
    public float steerStiffness = 0.1f;
    [Header("WheelColliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelBL;
    public WheelCollider wheelBR;
    [Header("Wheel Renderer Transforms")]
    public Transform flWheel;
    public Transform frWheel;
    public Transform rlWheel;
    public Transform rrWheel;

    [Header("Lights")]
    public Light headLightL;
    public Light headLightR;
    public Light rearLightL;
    public Light rearLightR;
    
    bool headLightStat = true;
    [Header("Components")]
    public Transform CenterOfMass;
    public Transform frontCamPos;
    public Rigidbody rb;
    public Renderer rend;
    public Transform axlePosition;
    public Transform frontSensor;
    public bool randomColor = true;

    //AI navigation
    float F = 5000;
    float nodeNearDistance = 8f;
    public Node targetNode;
    Vector3 dir;
    float speed;
    Node prevNode;
    float waitTime = 0;
    bool findingWay = false;
    int patienceLimit = 50;
    int rand = 0;
    float proximitySqr = 0;
    bool inView = true;
    float lastViewedTime = 0;
    [HideInInspector]
    public List<Node> path = null;

    [Header("Sensors")]
    public bool honked = false;
    public int skipFrames = 5;
    int frameCount = 0;
    public float width = 2;
    public float height = 0.5f;
    public float frontRange = 20;
    float distSqr = 625;
    [Tooltip("Layer to detect obstacles in way")]
    public LayerMask layerMask;
    [Tooltip("Layer to detect node (SpawnTriggers")]
    public LayerMask nodeLayer;

    [Header("Honk")]
    public float honkRange = 15;
    public AudioSource honkSource;
    public float honkPitch = 1;
    public LayerMask honkLayer;
    float honkRespondTime = 0;


    public void OnSpawn()
    {
        // called when vehicle is spawned
        findingWay = false;
        lastViewedTime = Time.time;
        if(randomColor)
            rend.material.SetColor("_Color", RandomColor());
    }

    public Color RandomColor()
    {
        return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
    }

    private void OnBecameInvisible()
    {
        inView = false;
    }
    private void OnBecameVisible()
    {
        inView = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (CenterOfMass == null)
            rb.centerOfMass = Vector3.zero;
        else rb.centerOfMass = CenterOfMass.position;
        if (controlType == ControlType.playerControlled) rb.drag = 0;
        SwitchHeadlights();
        rand = UnityEngine.Random.Range(0, 10);
        //rb = GetComponent<Rigidbody>();
        //rend = GetComponent<Renderer>();
    }
    public void Update()
    {
        //speed = Mathf.Lerp(speed, wheelBL.rpm * (Mathf.PI / 30) * wheelBL.radius * 100, Time.deltaTime);
        speed = Vector3.Dot(rb.velocity, transform.forward);
    }

    void OnDrawGizmosSelected()
    {
        if (frontSensor == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(frontSensor.position - width * 0.5f * transform.right, frontSensor.position + width * 0.5f * transform.right);
        Gizmos.DrawLine(frontSensor.position, frontSensor.position + height* transform.up);
    }

    public void FixedUpdate()
    {
        //Apply drag force
        if (controlType!=ControlType.Parked)
        {
            damping = (rb.mass * acceleration) / maxSpeed;
            rb.AddForce(-rb.velocity * damping);
        }
        
        //### VEHICLE NODE BASED AI ###
        if (controlType == ControlType.AI_FreeRoam || controlType == ControlType.AI_FollowPath)
        {
            if (true)
            {
                // check if vehicle gets too far from camera
                proximitySqr = (transform.position - PlayerController.mainCam.position).sqrMagnitude;
                if (proximitySqr > VehicleSpawner.dissapearDistSqr)
                {
                    Dissapear();
                }

                if (inView) lastViewedTime = Time.time;

                // Sensors
                if (targetNode != null)
                {
                    dir = transform.InverseTransformDirection(targetNode.transform.position - axlePosition.position);
                    // front boxcast and get min distance squared once every 'skipFrames' no. of frames
                    if (frameCount == skipFrames)
                    {
                        frameCount = 0;
                        RaycastHit hit;
                        distSqr = (frontRange * frontRange);
                        //float cosA = Vector3.Dot(transform.forward, (targetNode.transform.position - (transform.position + frontOffset * transform.forward)).normalized);
                        Vector3 raycastDir = (targetNode.transform.position - frontSensor.position);
                        if (Vector3.Dot(raycastDir, transform.forward) > 0)
                        {
                            float sensorWidth = Mathf.Lerp(width, 20, (speed-5f) / 20);//width + Mathf.Clamp(speed / 20, 0, 10);
                            if (Physics.BoxCast(frontSensor.position+transform.up*0.5f*height, new Vector3(sensorWidth * 0.5f, height*0.5f, 0.25f), raycastDir, out hit, transform.rotation, frontRange, layerMask))
                            {
                                distSqr = (hit.point - axlePosition.position).sqrMagnitude;
                                if (Vector3.Dot(hit.transform.forward, transform.forward) < 0  && Time.time > honkRespondTime + 5 + rand)
                                {
                                    if (rand < 3 && distSqr < 100)
                                    {
                                        Honk();
                                        honkRespondTime = Time.time;
                                    }
                                    else if (distSqr < 12 && speed<0.05f && Time.time > waitTime+8)
                                    {
                                        StartCoroutine(BackOff());
                                        waitTime = Time.time;
                                    }
                                }
                                Debug.DrawLine(hit.point, frontSensor.position);
                            }
                        }

                    }
                    frameCount += 1;
                }

                // When vehicle is far from the camera (AI LOD1)
                if(proximitySqr>VehicleSpawner.physicsLODDistSqr)
                {
                    if (wheelBL.enabled)
                    {
                        wheelBL.enabled = false;
                        wheelBR.enabled = false;
                        wheelFL.enabled = false;
                        wheelFR.enabled = false;
                    }

                    if (targetNode == null)
                    {
                        rb.velocity = Vector3.zero;
                    }
                    else
                    {
                        #region SteerAI_DirectVelocityBased
                        bool brake = distSqr < 50 || !targetNode.signal;
                        Vector3 dir1 = targetNode.transform.position - transform.position;
                        desiredVel = brake ? 0 : targetNode.speedLimit * 0.15f;
                        rb.velocity = desiredVel * dir1.normalized;
                        rb.velocity = rb.velocity + Physics.gravity * Time.fixedDeltaTime;
                        if (!brake && dir1.sqrMagnitude > 25)
                        {
                            rb.rotation = Quaternion.LookRotation(dir1);
                        }
                        #endregion

                        if (Time.time > lastViewedTime + 1 + rand)
                        {
                            Dissapear();
                        }

                        // select next node
                        if ((targetNode.transform.position - axlePosition.position).sqrMagnitude < nodeNearDistance)
                        {
                            SelectNextNode();
                        }

                    }
                }
                
                // For nearer vehicles (AI LOD0)
                else
                {
                    if (!wheelBL.enabled)
                    {
                        wheelBL.enabled = true;
                        wheelBR.enabled = true;
                        wheelFL.enabled = true;
                        wheelFR.enabled = true;

                        rb.velocity = desiredVel * transform.forward;
                    }

                    if (targetNode == null)
                    {
                        // if theres no where to go-->break and stop
                        wheelBL.brakeTorque = rb.mass * 50f * speed * speed;
                        wheelBR.brakeTorque = rb.mass * 50f * speed * speed;
                    }
                    else
                    {
                        #region SteerAI_WheelColliderBased

                        float desiredMaxSpeed = targetNode.speedLimit * (5f / 18f);
                        float nodeDistSqr = (targetNode.transform.position - axlePosition.position).sqrMagnitude;
                        //ensure stopage at no signal nodes
                        if (!targetNode.signal && nodeDistSqr < distSqr)
                        {
                            distSqr = nodeDistSqr;
                        }
                        desiredVel = Mathf.Lerp(2.5f, desiredMaxSpeed, distSqr / (frontRange * frontRange));

                        bool blockade = distSqr < 16;
                        if (!blockade) waitTime = Time.time;
                        if (Time.time > waitTime + patienceLimit + rand && !findingWay)
                        {
                            StartCoroutine(FindWay());
                        }
                        if (!findingWay)
                        {
                            float ang = Mathf.Atan(dir.x / Mathf.Abs(dir.z)) * Mathf.Rad2Deg;
                            bool brake = (speed > desiredVel) || blockade;// || !targetNode.signal;
                            bool acc = !brake;//&& (speed < 0.8f * desiredVel);
                            bool reorient = (Mathf.Abs(ang) > 85 || dir.z < 0);
                            wheelBL.brakeTorque = 0;
                            wheelBR.brakeTorque = 0;
                            F = rb.mass * acceleration;
                            //Debug.Log((speed > 1.2f * targetNode.speedLimit * (5f / 18f)).ToString() + " " + blockade);
                            if (acc)
                            {
                                if (nodeDistSqr > 200)
                                {
                                    wheelBR.motorTorque = F * (1 - (speed / (8)));
                                    wheelBL.motorTorque = F * (1 - (speed / (8)));
                                }
                                else if (reorient && wheelBL.rpm > 1)
                                {
                                    wheelBR.motorTorque = -F * (1 - (brakefactor * Mathf.Abs(speed) / desiredVel));
                                    wheelBL.motorTorque = -F * (1 - (brakefactor * Mathf.Abs(speed) / desiredVel));
                                }
                                else if (reorient)
                                {
                                    wheelBL.brakeTorque = 0;
                                    wheelBR.brakeTorque = 0;
                                    wheelBR.motorTorque = -brakefactor * F * (1 - (Mathf.Abs(speed) / (brakefactor * 10)));
                                    wheelBL.motorTorque = -brakefactor * F * (1 - (Mathf.Abs(speed) / (brakefactor * 10)));
                                }
                                else
                                {
                                    wheelBL.brakeTorque = 0;
                                    wheelBR.brakeTorque = 0;
                                    wheelBR.motorTorque = F * (1 - (speed / (0.8f * desiredVel)));
                                    wheelBL.motorTorque = F * (1 - (speed / (0.8f * desiredVel)));
                                }
                            }
                            else
                            {
                                wheelBL.motorTorque = 0;
                                wheelBR.motorTorque = 0;
                            }
                            if (brake)
                            {
                                wheelBL.brakeTorque = rb.mass * 50f * speed * speed;
                                wheelBR.brakeTorque = rb.mass * 50f * speed * speed;

                            }
                            if (blockade)
                            {
                                SwitchRearLights(true);
                            }
                            else
                            {
                                SwitchRearLights(false);
                            }
                            Steer((reorient && nodeDistSqr < 200) ? 0 : Mathf.Sign(speed) * Mathf.Clamp(ang, -60, 60));
                        }
                        #endregion

                        if (inView) UpdateWheelMesh();

                        // select next node
                        if ((targetNode.transform.position - axlePosition.position).sqrMagnitude < (findingWay ? 40 : nodeNearDistance + speed * 0.2f - 1))
                        {
                            SelectNextNode();
                        }
                    }
                }
            }
        }

        // parked vehicles
        else if(controlType == ControlType.Parked)
        {
            if (!wheelBL.enabled)
            {
                wheelBL.enabled = true;
                wheelBR.enabled = true;
                wheelFL.enabled = true;
                wheelFR.enabled = true;
            }

            proximitySqr = (transform.position - PlayerController.mainCam.position).sqrMagnitude;
            if (inView && proximitySqr< VehicleSpawner.physicsLODDistSqr)
                UpdateWheelMesh();
            else if (proximitySqr > VehicleSpawner.dissapearDistSqr)
            {
                Dissapear();
            }
        }
    }

    public void SelectNextNode()
    {
        if(controlType == ControlType.AI_FollowPath)
        {
            if (path!=null)
            {
                path.Remove(targetNode);
                if (path.Count > 0)
                    targetNode = path[0];
                else targetNode = null;
            }

            return;
        }

        //if (!targetNode.signal) return;
        if (targetNode.branches.Count>0)
        {
            int rand = UnityEngine.Random.Range(0, targetNode.branches.Count);
            targetNode = targetNode.branches[rand];
        }
        else
        {
            targetNode = null;
        }
        if (prevNode != null && targetNode!=null)
        {
                if (honked)
                {
                    foreach (Node n in prevNode.branches)
                    {
                        if (n.nodeType == Node.NodeType.divert)
                        {
                            honked = false;
                            targetNode = n;
                            break;
                        }
                    }
                }
                else if (targetNode.nodeType == Node.NodeType.divert || targetNode.nodeType == Node.NodeType.uTurn)
                {
                    foreach (Node n in prevNode.branches)
                    {
                        if (n.nodeType != Node.NodeType.divert && n.nodeType != Node.NodeType.uTurn)
                        {
                            targetNode = n;
                            break;
                        }
                    }
                }
            }
            prevNode = targetNode;
     
    }

    public void Move(float steer, bool accelerate,bool reverse,bool brake)
    {
        Accelerate(accelerate,reverse,brake);
        Steer(steer);
    }

    public void Accelerate(bool acc,bool rev,bool brake)
    {
        wheelBL.motorTorque = 0;
        wheelBR.motorTorque = 0;
        wheelBL.brakeTorque = 0;
        wheelBR.brakeTorque = 0;
        wheelFL.brakeTorque = 0;
        wheelFR.brakeTorque = 0;

        float F = rb.mass * acceleration;
            
        if (Vector3.Dot(rb.velocity,transform.forward)>=0)
        {
            if (acc)
            {
                wheelBR.motorTorque = F * (1 - (speed / maxSpeed))*Time.deltaTime * 30;
                wheelBL.motorTorque = F * (1 - (speed / maxSpeed)) * Time.deltaTime * 30;
            }
            if (rev)
            {
                if (controlType == ControlType.playerControlled)
                {
                    wheelBL.brakeTorque = 20000000000 + 5000000 * Mathf.Abs(rb.mass * wheelBL.rpm * brakefactor * acceleration * Time.deltaTime * 60);
                    wheelBR.brakeTorque = 20000000000 + 5000000 * Mathf.Abs(rb.mass * wheelBR.rpm * acceleration * brakefactor * Time.deltaTime * 60);
                }

                SwitchRearLights(true);
            }
            else
            {
                SwitchRearLights(false);
            }
        }
        else
        {
            if (acc)
            {
                if (controlType == ControlType.playerControlled)
                {
                    wheelBL.brakeTorque = 20000000000 + 5000000 * Mathf.Abs(rb.mass * wheelBL.rpm * brakefactor * acceleration * Time.deltaTime * 30);
                    wheelBR.brakeTorque = 20000000000 + 5000000 * Mathf.Abs(rb.mass * wheelBR.rpm * acceleration * brakefactor * Time.deltaTime * 30);
                }
            }
            if (rev)
            {
                wheelBR.motorTorque = -F*(1 - Mathf.Abs(speed * brakefactor / maxSpeed)) * Time.deltaTime * 30;
                wheelBL.motorTorque = -F*(1 - Mathf.Abs(speed * brakefactor / maxSpeed)) * Time.deltaTime * 30;
            }
        }
        if (brake)
        {
            //wheelFL.brakeTorque = 8000000000 * Time.deltaTime * 60*rb.mass*brakefactor;
            //wheelFR.brakeTorque = 8000000000 * Time.deltaTime * 60*rb.mass*brakefactor;
            WheelFrictionCurve fc = wheelBL.sidewaysFriction;
            fc.stiffness = Mathf.Lerp(fc.stiffness,driftGripReduction * tireGrip,Time.deltaTime);
            wheelBL.sidewaysFriction = fc;
            fc = wheelBR.sidewaysFriction;
            fc.stiffness = Mathf.Lerp(fc.stiffness,driftGripReduction * tireGrip,Time.deltaTime);
            wheelBR.sidewaysFriction = fc;
        }
        else
        {
            WheelFrictionCurve fc = wheelBL.sidewaysFriction;
            fc.stiffness = Mathf.Lerp(fc.stiffness, tireGrip, Time.deltaTime);
            wheelBL.sidewaysFriction = fc;
            fc = wheelBR.sidewaysFriction;
            fc.stiffness = Mathf.Lerp(fc.stiffness, tireGrip, Time.deltaTime);
            wheelBR.sidewaysFriction = fc;
        }

        if(!rev && !acc) {
            wheelBL.motorTorque = 0;
            wheelBR.motorTorque = 0;
        }

        UpdateWheelMesh();
    }

    public void Steer(float x)
    {
        if (controlType == ControlType.playerControlled)
        {
            wheelFL.steerAngle = (x * maxSteer) / Mathf.Abs(0.01f * wheelFL.rpm * steerStiffness + 1);
            wheelFR.steerAngle = (x * maxSteer) / Mathf.Abs(0.01f * wheelFL.rpm * steerStiffness + 1);
        }
        else
        {
            wheelFL.steerAngle = x;
            wheelFR.steerAngle = x;
        }
    }
    
    public void FollowPath(List<Node> path)
    {
        controlType = ControlType.AI_FollowPath;
        this.path = path;
        if (path != null)
        {
            if (path.Count > 0)
                targetNode = path[0];
        }
    }

    public void UpdateWheelMesh()
    {
        flWheel.localEulerAngles = new Vector3(flWheel.localEulerAngles.x, wheelFL.steerAngle - flWheel.localEulerAngles.z, flWheel.localEulerAngles.z);
        frWheel.localEulerAngles = flWheel.localEulerAngles;

        flWheel.Rotate(wheelFL.rpm*2*Time.fixedDeltaTime, 0, 0);
        frWheel.Rotate(wheelFR.rpm * 2 * Time.fixedDeltaTime, 0, 0);
        rlWheel.Rotate(wheelBL.rpm * 2 * Time.fixedDeltaTime, 0, 0);
        if(rrWheel!=null)
            rrWheel.Rotate(wheelBR.rpm * 2 * Time.fixedDeltaTime, 0, 0);
    }

    public void SwitchHeadlights()
    {
        if (controlType == ControlType.playerControlled)
            headLightStat = !headLightStat;
        else if (controlType != ControlType.Parked) headLightStat = !TimeCycle.isDay;
        else headLightStat = false;

        if (headLightStat)
        { // switch on 
            if (headLightL.gameObject.activeInHierarchy)
            {
                headLightL.enabled = true;
                rend.material.SetFloat("HeadLight", 1);
            }
            if (headLightR.gameObject.activeInHierarchy)
            {
                headLightR.enabled = true;
                rend.material.SetFloat("HeadLight", 1);
            }
        }
        else
        {// switch off
            headLightL.enabled = false;
            rend.material.SetFloat("HeadLight", 0);
            headLightR.enabled = false;
            rend.material.SetFloat("HeadLight", 0);
        }
    }

    public void SwitchHeadLights(bool stat)
    {
        if (stat)
        { // switch on 
            if (headLightL.gameObject.activeInHierarchy)
            {
                headLightL.enabled = true;
                rend.material.SetFloat("HeadLight", 1);
            }
            if (headLightR.gameObject.activeInHierarchy)
            {
                headLightR.enabled = true;
                rend.material.SetFloat("HeadLight", 1);
            }
        }
        else
        {// switch off
            headLightL.enabled = false;
            rend.material.SetFloat("HeadLight", 0);
            headLightR.enabled = false;
            rend.material.SetFloat("HeadLight", 0);
        }
    }

    public void SwitchRearLights(bool switchOn)
    {
        if (switchOn)
        {
            if (rearLightL.gameObject.activeInHierarchy && !rearLightL.enabled)
            { rearLightL.enabled = true; rend.material.SetFloat("RearLight", 1); }
            if (rearLightR.gameObject.activeInHierarchy && !rearLightR.enabled)
            { rearLightR.enabled = true; rend.material.SetFloat("RearLight", 1); }
        }
        else
        {
            if (rearLightL.gameObject.activeInHierarchy && rearLightL.enabled)
            { rearLightL.enabled = false; rend.material.SetFloat("RearLight", 0); }
            if (rearLightR.gameObject.activeInHierarchy && rearLightR.enabled)
            { rearLightR.enabled = false; rend.material.SetFloat("RearLight", 0); }
        }
    }

    public void Honk()
    {
        honkSource.pitch = honkPitch;
        if(!honkSource.isPlaying)
            honkSource.Play();
        if(false)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, honkRange, honkLayer);
            foreach (Collider col in cols)
            {
                IHonkable h = col.GetComponent<IHonkable>();
                if (h != null)
                {
                    h.OnHonked(transform);
                }
            }

        }
        RaycastHit hit;
        if(Physics.BoxCast(axlePosition.position,new Vector3(2,1,1),transform.forward,out hit, transform.rotation, 20, honkLayer))
        {
            IHonkable h = hit.transform.GetComponent<IHonkable>();
            if (h != null)
            {
                h.OnHonked(transform);
            }
        }
    }

    public void OnHonked(Transform honker)
    {
        if (true)//Vector3.Dot(transform.position - honker.position, honker.forward) > 0 && Vector3.Dot(honker.forward, rb.velocity) > 0)
        {
            if (Time.time > honkRespondTime + 10)
            {
                honkRespondTime = Time.time;
                honked = true;
            }
        }
    }

    public void SetInitialTransform(Node node)
    {
        if(node.nodeType == Node.NodeType.parking)
        {
            transform.rotation = node.transform.rotation;
            transform.position = node.transform.position;
            SwitchHeadlights();
            SwitchRearLights(false);
            controlType = ControlType.Parked;
            wheelBL.brakeTorque = 1;
            wheelBR.brakeTorque = 1;
            rb.velocity = Vector3.zero;
            return;
        }

        if (node.branches[0] != null)
        {
            controlType = ControlType.AI_FreeRoam;
            transform.rotation = Quaternion.LookRotation(node.branches[0].transform.position - node.transform.position);
            transform.position = node.transform.position + transform.position - axlePosition.position;
            targetNode = node.branches[0];
            rb.velocity = targetNode.speedLimit * 0.15f * transform.forward;
            prevNode = node;
        }
        else
        {
            StartCoroutine(AutoFindRandomNode());
            prevNode = node;
        }
        //SelectNextRandomNode();
    }
    
    IEnumerator AutoFindRandomNode()
    {
        yield return null;
        yield return new WaitForFixedUpdate();
        Collider[] cols = Physics.OverlapSphere(transform.position, 8, nodeLayer);
        Node n = null;
        for (int i = 0; i < cols.Length; i++)
        {
            Collider col = cols[i];
            if (Vector3.Dot(col.transform.forward, transform.forward) > 0)
            {
                n = col.GetComponent<Node>();
                if (n != null)
                {
                    targetNode = n;
                }
            }
        }
    }

    IEnumerator FindWay()
    {
        findingWay = true;
        wheelBL.brakeTorque = rb.mass * 50f * speed * speed;
        wheelBR.brakeTorque = rb.mass * 50f * speed * speed;
        Honk();
        yield return new WaitForSeconds(2);
        float startTime = Time.time;
        F = rb.mass * acceleration;
        desiredVel = 3;
        //bool hit = true;
        while (Time.time<startTime+1)// && distSqr<36)
        {
            yield return new WaitForFixedUpdate();
            //hit = (Physics.Raycast(axlePosition.position - width * transform.right, transform.forward, 6, layerMask)) ;
            Steer(60);
            wheelBL.brakeTorque = 0;
            wheelBR.brakeTorque = 0;
            wheelBR.motorTorque = F * (1 - (speed / desiredVel));
            wheelBL.motorTorque = F * (1 - (speed / desiredVel));
        }
        while (Time.time < startTime + 2)// && distSqr<36)
        {
            yield return new WaitForFixedUpdate();
            //hit = (Physics.Raycast(axlePosition.position - width * transform.right, transform.forward, 6, layerMask)) ;
            Steer(-30);
            wheelBL.brakeTorque = 0;
            wheelBR.brakeTorque = 0;
            wheelBR.motorTorque = F * (1 - (speed / desiredVel));
            wheelBL.motorTorque = F * (1 - (speed / desiredVel));
        }
        //SelectNextNode();
        findingWay = false;
    }
    IEnumerator BackOff()
    {
        Debug.Log("Backing");
        findingWay = true;
        float startTime = Time.time;
        F = rb.mass * acceleration;
        //bool hit = true;
        while (Time.time < startTime + rand*0.1f+0.5f)
        {
            yield return new WaitForFixedUpdate();
            //hit = (Physics.Raycast(axlePosition.position - width * transform.right, transform.forward, 6, layerMask)) ;
            Steer(0);
            desiredVel = -0.5f-0.15f*rand;
            wheelBL.brakeTorque = 0;
            wheelBR.brakeTorque = 0;
            wheelBR.motorTorque = -F * (1 - (speed / desiredVel));
            wheelBL.motorTorque = -F * (1 - (speed / desiredVel));
        }
        //SelectNextNode();
        findingWay = false;
    }

    private void Dissapear()
    {
        inView = true;
        findingWay = false;
        honked = false;
        proximitySqr = 0;
        StopAllCoroutines();
        //targetNode = null;
        VehicleSpawner.spawnedVeh -= 1;
        ObjectPooler.instance.StoreVehInPool("Vehicles", gameObject);
    }

}
