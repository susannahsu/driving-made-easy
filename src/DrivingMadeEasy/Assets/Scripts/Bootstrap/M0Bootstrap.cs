using System.Collections.Generic;
using UnityEngine;
using DrivingMadeEasy.Input;
using DrivingMadeEasy.Vehicle;
using DrivingMadeEasy.CameraRig;
using DrivingMadeEasy.UI;

namespace DrivingMadeEasy.Bootstrap
{
    /// <summary>
    /// Builds the entire M0 "empty parking lot" scene in code at runtime: ground, a
    /// drivable car (Rigidbody + 4 WheelColliders + visual wheels + tilt steering +
    /// forgiving physics), a slalom of cones, a chase camera, and the HUD.
    ///
    /// Why procedural? It keeps M0 entirely in reviewable C# (good for the portfolio)
    /// and removes the need to hand-author a fragile binary .unity scene. To run M0:
    /// create an empty scene, add one empty GameObject, attach this component, press
    /// Play (or build to the iPhone). See docs/m0-setup.md.
    /// </summary>
    public class M0Bootstrap : MonoBehaviour
    {
        [Header("Lot")]
        public float lotSize = 120f;

        [Header("Slalom")]
        public int coneCount = 8;
        public float coneSpacing = 9f;

        private void Start()
        {
            BuildGround();
            GameObject car = BuildCar(new Vector3(0f, 0.6f, -20f));
            BuildCamera(car.transform);
            BuildCones();
        }

        // ---- Ground ---------------------------------------------------------------

        private void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "ParkingLot";
            ground.transform.localScale = Vector3.one * (lotSize / 10f); // Plane is 10u
            ground.GetComponent<Renderer>().material.color = new Color(0.18f, 0.18f, 0.20f);

            // A bright "start" stripe so the player has a reference line.
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "StartStripe";
            stripe.transform.position = new Vector3(0f, 0.02f, -26f);
            stripe.transform.localScale = new Vector3(10f, 0.02f, 0.4f);
            stripe.GetComponent<Renderer>().material.color = Color.white;
            Destroy(stripe.GetComponent<BoxCollider>());
        }

        // ---- Car ------------------------------------------------------------------

        private GameObject BuildCar(Vector3 position)
        {
            var car = new GameObject("Car");
            car.transform.position = position;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(car.transform, false);
            body.transform.localScale = new Vector3(1.8f, 0.8f, 4.2f);
            body.GetComponent<Renderer>().material.color = new Color(0.85f, 0.2f, 0.2f);
            Destroy(body.GetComponent<BoxCollider>()); // the Rigidbody uses wheel + a hull collider

            var hull = car.AddComponent<BoxCollider>();
            hull.center = new Vector3(0f, 0.3f, 0f);
            hull.size = new Vector3(1.8f, 0.8f, 4.2f);

            var rb = car.AddComponent<Rigidbody>();
            rb.mass = 1200f;
            rb.drag = 0.05f;
            rb.angularDrag = 0.3f;

            // Wheels: front axle steers, rear axle drives.
            var fl = BuildWheel(car.transform, new Vector3(-0.9f, 0f, 1.5f), "FL");
            var fr = BuildWheel(car.transform, new Vector3(0.9f, 0f, 1.5f), "FR");
            var rl = BuildWheel(car.transform, new Vector3(-0.9f, 0f, -1.5f), "RL");
            var rr = BuildWheel(car.transform, new Vector3(0.9f, 0f, -1.5f), "RR");

            var input = car.AddComponent<MotionSteeringInput>();
            var controller = car.AddComponent<CarController>();
            controller.driverInputSource = input;
            controller.axles = new[]
            {
                new CarController.Axle
                {
                    leftWheel = fl.collider, rightWheel = fr.collider,
                    leftMesh = fl.mesh, rightMesh = fr.mesh,
                    motor = false, steering = true
                },
                new CarController.Axle
                {
                    leftWheel = rl.collider, rightWheel = rr.collider,
                    leftMesh = rl.mesh, rightMesh = rr.mesh,
                    motor = true, steering = false
                }
            };

            return car;
        }

        private (WheelCollider collider, Transform mesh) BuildWheel(
            Transform parent, Vector3 localPos, string label)
        {
            var anchor = new GameObject($"Wheel_{label}");
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = localPos;

            var collider = anchor.AddComponent<WheelCollider>();
            collider.radius = 0.35f;
            collider.suspensionDistance = 0.2f;
            collider.center = new Vector3(0f, -0.1f, 0f);

            // Forgiving suspension + high grip so the car stays planted (SPEC §12).
            var spring = collider.suspensionSpring;
            spring.spring = 35000f;
            spring.damper = 4500f;
            spring.targetPosition = 0.5f;
            collider.suspensionSpring = spring;

            var fwd = collider.forwardFriction;  fwd.stiffness = 2.2f; collider.forwardFriction = fwd;
            var side = collider.sidewaysFriction; side.stiffness = 2.6f; collider.sidewaysFriction = side;

            // Visual wheel (a flattened cylinder), purely cosmetic.
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mesh.name = $"WheelMesh_{label}";
            Destroy(mesh.GetComponent<CapsuleCollider>());
            mesh.transform.SetParent(parent, false);
            mesh.transform.localScale = new Vector3(0.7f, 0.1f, 0.7f);
            mesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            mesh.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);

            return (collider, mesh.transform);
        }

        // ---- Camera & HUD ---------------------------------------------------------

        private void BuildCamera(Transform carTransform)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            var chase = cam.gameObject.AddComponent<ChaseCamera>();
            chase.target = carTransform;

            var hud = new GameObject("HUD").AddComponent<DrivingHud>();
            hud.car = carTransform.GetComponent<CarController>();
            hud.driverInputSource = carTransform.GetComponent<MotionSteeringInput>();
        }

        // ---- Cones -----------------------------------------------------------------

        private void BuildCones()
        {
            var cones = new List<GameObject>();
            for (int i = 0; i < coneCount; i++)
            {
                var cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cone.name = $"Cone_{i}";
                cone.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
                float x = (i % 2 == 0) ? -1.5f : 1.5f; // weave left/right for a slalom
                cone.transform.position = new Vector3(x, 0.5f, -10f + i * coneSpacing);
                cone.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);
                cones.Add(cone);
            }
        }
    }
}
