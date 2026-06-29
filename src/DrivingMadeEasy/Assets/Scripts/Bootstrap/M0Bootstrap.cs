using System.Collections.Generic;
using UnityEngine;
using DrivingMadeEasy.Input;
using DrivingMadeEasy.Vehicle;
using DrivingMadeEasy.CameraRig;
using DrivingMadeEasy.UI;
using DrivingMadeEasy.Game;

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
            BuildRoad();
            GameObject car = BuildCar(new Vector3(0f, 0.6f, -20f));
            BuildCamera(car.transform);
            CoachRuntime coach = BuildCoach();
            BuildStopSign(coach);
            BuildSpeedZone(coach);
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

            // Visual wheel: SyncMesh drives the *holder* with WheelCollider.GetWorldPose
            // (which includes the rolling spin). The cylinder is a child carrying a fixed
            // 90° offset so its round face points along the axle — a raw cylinder's long
            // axis is its local Y, which would otherwise point forward.
            var holder = new GameObject($"WheelMesh_{label}");
            holder.transform.SetParent(parent, false);

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mesh.name = $"WheelMeshVisual_{label}";
            Destroy(mesh.GetComponent<CapsuleCollider>());
            mesh.transform.SetParent(holder.transform, false);
            mesh.transform.localScale = new Vector3(0.7f, 0.1f, 0.7f);
            mesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            mesh.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);

            return (collider, holder.transform);
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

            var driverCam = cam.gameObject.AddComponent<DriverCamera>();
            driverCam.target = carTransform;
            driverCam.view = DriverCamera.View.Cockpit; // start in the driver's seat

            var hud = new GameObject("HUD").AddComponent<DrivingHud>();
            hud.car = carTransform.GetComponent<CarController>();
            hud.driverInputSource = carTransform.GetComponent<MotionSteeringInput>();
        }

        // ---- Road (M2 scaffolding) -------------------------------------------------

        private void BuildRoad()
        {
            // A straight asphalt strip painted on the lot so it reads as a street. Purely
            // visual — the car still drives on the ground plane's collider.
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Road";
            road.transform.localScale = new Vector3(7f, 0.04f, 170f);
            road.transform.position = new Vector3(0f, 0.02f, 50f);
            road.GetComponent<Renderer>().material.color = new Color(0.12f, 0.12f, 0.13f);
            Destroy(road.GetComponent<BoxCollider>());

            for (int s = -1; s <= 1; s += 2)
            {
                var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                edge.name = "RoadEdge";
                edge.transform.localScale = new Vector3(0.15f, 0.05f, 170f);
                edge.transform.position = new Vector3(3.3f * s, 0.05f, 50f);
                edge.GetComponent<Renderer>().material.color = Color.white;
                Destroy(edge.GetComponent<BoxCollider>());
            }

            for (int i = 0; i < 28; i++)
            {
                var dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dash.name = "CenterDash";
                dash.transform.localScale = new Vector3(0.15f, 0.05f, 2f);
                dash.transform.position = new Vector3(0f, 0.05f, -30f + i * 6f);
                dash.GetComponent<Renderer>().material.color = new Color(0.9f, 0.85f, 0.2f);
                Destroy(dash.GetComponent<BoxCollider>());
            }
        }

        // ---- Coach + stop sign (M1) ------------------------------------------------

        private CoachRuntime BuildCoach()
        {
            var go = new GameObject("Coach");
            var runtime = go.AddComponent<CoachRuntime>();

            var hud = go.AddComponent<CoachHud>();
            hud.coach = runtime;          // CoachHud subscribes in Start(), after this runs
            hud.trackedRuleId = "stop_sign";

            var report = go.AddComponent<DriveReportHud>();
            report.coach = runtime;
            return runtime;
        }

        private void BuildStopSign(CoachRuntime coach)
        {
            const float zLine = -6f;   // where the stop line sits
            const float sideX = 2.4f;  // sign stands to the right of the lane

            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "StopSignPole";
            pole.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
            pole.transform.position = new Vector3(sideX, 1.0f, zLine);
            pole.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
            Destroy(pole.GetComponent<CapsuleCollider>());

            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "StopSign";
            sign.transform.localScale = new Vector3(0.7f, 0.7f, 0.08f);
            sign.transform.position = new Vector3(sideX, 1.9f, zLine);
            sign.GetComponent<Renderer>().material.color = new Color(0.8f, 0.05f, 0.05f);
            Destroy(sign.GetComponent<BoxCollider>());

            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "StopLine";
            line.transform.localScale = new Vector3(6f, 0.02f, 0.4f);
            line.transform.position = new Vector3(0f, 0.02f, zLine);
            line.GetComponent<Renderer>().material.color = Color.white;
            Destroy(line.GetComponent<BoxCollider>());

            // Trigger zone: the approach plus the line. Car enters ~7m before the line and
            // exits a couple of metres past it; StopSignZone judges the stop in between.
            var zoneGo = new GameObject("StopSignZone");
            zoneGo.transform.position = new Vector3(0f, 1f, zLine - 2.5f);
            var box = zoneGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(6f, 3f, 9f);
            var zone = zoneGo.AddComponent<StopSignZone>();
            zone.coach = coach;
            zone.ruleId = "stop_sign";
        }

        private void BuildSpeedZone(CoachRuntime coach)
        {
            const int limitMph = 20;
            const float zSign = 2f;       // posted limit sign
            const float zCenter = 20f;    // middle of the enforced stretch
            const float zLength = 36f;    // stretch spans ~z 2..38 — long enough to manage speed

            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "SpeedSignPole";
            pole.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
            pole.transform.position = new Vector3(2.4f, 1.0f, zSign);
            pole.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
            Destroy(pole.GetComponent<CapsuleCollider>());

            // White "speed limit" sign (the number is read out by the Coach + HUD for now).
            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "SpeedLimitSign";
            sign.transform.localScale = new Vector3(0.6f, 0.8f, 0.08f);
            sign.transform.position = new Vector3(2.4f, 1.95f, zSign);
            sign.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.95f);
            Destroy(sign.GetComponent<BoxCollider>());

            var zoneGo = new GameObject("SpeedZone");
            zoneGo.transform.position = new Vector3(0f, 1f, zCenter);
            var box = zoneGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(6f, 3f, zLength);
            var zone = zoneGo.AddComponent<SpeedZone>();
            zone.coach = coach;
            zone.ruleId = "speed_limit";
            zone.limitMph = limitMph;
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
                // Start the slalom past the stop sign and speed-limit stretch.
                cone.transform.position = new Vector3(x, 0.5f, 46f + i * coneSpacing);
                cone.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);
                cones.Add(cone);
            }
        }
    }
}
