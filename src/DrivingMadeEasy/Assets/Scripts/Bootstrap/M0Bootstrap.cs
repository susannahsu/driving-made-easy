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
        public int coneCount = 5;
        public float coneSpacing = 9f;

        private void Start()
        {
            ConfigureLighting();
            BuildGround();
            BuildRoad();
            BuildScenery();
            GameObject car = BuildCar(new Vector3(0f, 0.6f, -20f));
            BuildCamera(car.transform);
            CoachRuntime coach = BuildCoach();
            BuildStopSign(coach);
            BuildSpeedZone(coach);
            BuildCrosswalk(coach);
            BuildIntersection(coach);
            BuildCones();
        }

        // ---- Visual helpers --------------------------------------------------------

        /// A Standard-shader material (Built-in RP) with optional metallic / smoothness /
        /// emission, so props read as real surfaces instead of flat-shaded primitives.
        private static Material Mat(Color color, float metallic = 0f, float smoothness = 0.25f,
                                    Color? emission = null)
        {
            var m = new Material(Shader.Find("Standard"));
            m.color = color;
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Glossiness", smoothness);
            if (emission.HasValue)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission.Value);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            return m;
        }

        /// A colliderless cube with a given material (world space) — the scenery workhorse.
        private GameObject Box(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.position = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(g.GetComponent<BoxCollider>());
            return g;
        }

        /// A colliderless child cube placed in a parent's local space (e.g. car parts).
        private void AddPart(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name;
            g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(g.GetComponent<BoxCollider>());
        }

        /// A colliderless child cylinder (wheels, hub, etc.).
        private GameObject Cyl(Transform parent, string name, Vector3 localPos, Vector3 localScale,
                               Vector3 euler, Material mat)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            g.name = name;
            g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            g.transform.localScale = localScale;
            g.transform.localRotation = Quaternion.Euler(euler);
            g.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(g.GetComponent<CapsuleCollider>());
            return g;
        }

        // ---- People ----------------------------------------------------------------

        private Pedestrian BuildPerson(Vector3 pos, Color shirt)
        {
            var root = new GameObject("Pedestrian");
            root.transform.position = pos;

            var skin = Mat(new Color(0.85f, 0.7f, 0.6f));
            var shirtMat = Mat(shirt);
            var pantsMat = Mat(new Color(0.2f, 0.2f, 0.25f));

            AddPart(root.transform, "Torso", new Vector3(0f, 1.15f, 0f),
                    new Vector3(0.45f, 0.6f, 0.28f), shirtMat);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.66f, 0f);
            head.transform.localScale = new Vector3(0.34f, 0.36f, 0.34f);
            head.GetComponent<Renderer>().sharedMaterial = skin;
            Destroy(head.GetComponent<SphereCollider>());

            var ped = root.AddComponent<Pedestrian>();
            ped.leftLeg = LimbPivot(root.transform, new Vector3(-0.12f, 0.85f, 0f), pantsMat, 0.5f);
            ped.rightLeg = LimbPivot(root.transform, new Vector3(0.12f, 0.85f, 0f), pantsMat, 0.5f);
            ped.leftArm = LimbPivot(root.transform, new Vector3(-0.28f, 1.35f, 0f), shirtMat, 0.45f);
            ped.rightArm = LimbPivot(root.transform, new Vector3(0.28f, 1.35f, 0f), shirtMat, 0.45f);
            return ped;
        }

        private Transform LimbPivot(Transform parent, Vector3 localPos, Material mat, float length)
        {
            var pivot = new GameObject("Limb");
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = localPos;

            var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "Seg";
            seg.transform.SetParent(pivot.transform, false);
            seg.transform.localPosition = new Vector3(0f, -length * 0.5f, 0f);
            seg.transform.localScale = new Vector3(0.12f, length, 0.12f);
            seg.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(seg.GetComponent<BoxCollider>());
            return pivot.transform;
        }

        // ---- Steering wheel (cockpit) ----------------------------------------------

        private void BuildSteeringWheel(Transform car, CarController controller)
        {
            var pivot = new GameObject("SteeringWheel");
            pivot.transform.SetParent(car, false);
            pivot.transform.localPosition = new Vector3(-0.35f, 0.30f, 0.62f);
            pivot.transform.localRotation = Quaternion.Euler(-68f, 0f, 0f);

            var plastic = Mat(new Color(0.08f, 0.08f, 0.09f), 0.2f, 0.5f);
            const float R = 0.18f;
            const int seg = 16;
            for (int i = 0; i < seg; i++)
            {
                float a = (i / (float)seg) * Mathf.PI * 2f;
                var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
                s.name = "Rim";
                s.transform.SetParent(pivot.transform, false);
                s.transform.localPosition = new Vector3(Mathf.Cos(a) * R, Mathf.Sin(a) * R, 0f);
                s.transform.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg);
                float segLen = (2f * Mathf.PI * R / seg) * 1.3f;
                s.transform.localScale = new Vector3(0.035f, segLen, 0.05f);
                s.GetComponent<Renderer>().sharedMaterial = plastic;
                Destroy(s.GetComponent<BoxCollider>());
            }
            for (int k = 0; k < 3; k++)
            {
                float a = (-90f + k * 120f) * Mathf.Deg2Rad;
                var sp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sp.name = "Spoke";
                sp.transform.SetParent(pivot.transform, false);
                sp.transform.localPosition = new Vector3(Mathf.Cos(a) * R * 0.5f, Mathf.Sin(a) * R * 0.5f, 0f);
                sp.transform.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg - 90f);
                sp.transform.localScale = new Vector3(0.03f, R, 0.03f);
                sp.GetComponent<Renderer>().sharedMaterial = plastic;
                Destroy(sp.GetComponent<BoxCollider>());
            }
            Cyl(pivot.transform, "Hub", Vector3.zero, new Vector3(0.08f, 0.02f, 0.08f),
                new Vector3(90f, 0f, 0f), plastic);

            // Dashboard slab in front of the driver.
            AddPart(car, "Dashboard", new Vector3(0f, 0.25f, 1.0f), new Vector3(1.7f, 0.4f, 0.5f),
                    Mat(new Color(0.12f, 0.12f, 0.13f), 0.1f, 0.3f));

            pivot.AddComponent<SteeringWheelView>().car = controller;
        }

        // ---- Lighting & sky --------------------------------------------------------

        private void ConfigureLighting()
        {
            // A warm directional "sun" with soft shadows — the single biggest realism win.
            Light sun = null;
            foreach (var l in FindObjectsOfType<Light>())
                if (l.type == LightType.Directional) { sun = l; break; }
            if (sun == null)
            {
                sun = new GameObject("Sun").AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            sun.color = new Color(1f, 0.96f, 0.86f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;

            // Sky-toned ambient + gentle distance fog for depth.
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.46f, 0.48f, 0.46f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.26f, 0.22f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.74f, 0.8f, 0.86f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 240f;
        }

        // ---- Scenery (sidewalks, buildings, trees) ---------------------------------

        private void BuildScenery()
        {
            var sidewalkMat = Mat(new Color(0.62f, 0.62f, 0.62f), 0f, 0.1f);
            for (int s = -1; s <= 1; s += 2)
                Box("Sidewalk", new Vector3(4.6f * s, 0.06f, 50f), new Vector3(2.2f, 0.12f, 170f), sidewalkMat);

            var trunkMat = Mat(new Color(0.35f, 0.25f, 0.16f));
            var leafMat = Mat(new Color(0.24f, 0.45f, 0.22f));
            var windowMat = Mat(new Color(0.55f, 0.62f, 0.7f), 0.3f, 0.9f,
                                 new Color(0.35f, 0.38f, 0.32f)); // softly lit glass
            var doorMat = Mat(new Color(0.25f, 0.18f, 0.12f), 0.1f, 0.4f);
            var roofMat = Mat(new Color(0.2f, 0.2f, 0.22f), 0f, 0.2f);
            Color[] palette =
            {
                new Color(0.78f, 0.74f, 0.68f), new Color(0.70f, 0.60f, 0.55f),
                new Color(0.60f, 0.66f, 0.72f), new Color(0.82f, 0.78f, 0.70f),
                new Color(0.66f, 0.62f, 0.60f)
            };

            for (int s = -1; s <= 1; s += 2)
            {
                float x = 12f * s;
                for (int i = 0; i < 12; i++)
                {
                    float z = -28f + i * 14f;
                    if (Mathf.Abs(z - 78f) < 9f) continue; // keep the junction clear

                    if (i % 3 == 1)
                    {
                        Box("TreeTrunk", new Vector3(7f * s, 1f, z), new Vector3(0.4f, 2f, 0.4f), trunkMat);
                        var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        canopy.name = "TreeCanopy";
                        canopy.transform.position = new Vector3(7f * s, 2.7f, z);
                        canopy.transform.localScale = new Vector3(2.4f, 2.6f, 2.4f);
                        canopy.GetComponent<Renderer>().sharedMaterial = leafMat;
                        Destroy(canopy.GetComponent<SphereCollider>());
                    }
                    else
                    {
                        float h = 6f + ((i * 37) % 9); // varied heights, 6..14
                        var bMat = Mat(palette[(i + (s > 0 ? 2 : 0)) % palette.Length], 0f, 0.2f);
                        Box("Building", new Vector3(x, h * 0.5f, z), new Vector3(8f, h, 10f), bMat);

                        float faceX = x - 4.05f * s; // street-facing facade
                        Box("Roof", new Vector3(x, h + 0.15f, z), new Vector3(8.4f, 0.3f, 10.4f), roofMat);
                        Box("Door", new Vector3(faceX, 1.0f, z), new Vector3(0.12f, 2.0f, 1.4f), doorMat);

                        // A grid of lit windows on the street-facing facade.
                        int rows = Mathf.Clamp(Mathf.RoundToInt(h / 2.2f), 2, 6);
                        for (int rr = 0; rr < rows; rr++)
                        {
                            float wy = 2.6f + rr * 2.0f;
                            if (wy > h - 0.8f) continue;
                            for (int c = -1; c <= 1; c++)
                                Box("Window", new Vector3(faceX, wy, z + c * 2.6f),
                                    new Vector3(0.08f, 1.1f, 1.4f), windowMat);
                        }
                    }
                }
            }

            // A few people strolling the sidewalks for life.
            BuildPerson(new Vector3(4.6f, 0f, 12f), new Color(0.8f, 0.3f, 0.3f)).Configure(10f, 38f, true, 0.0f);
            BuildPerson(new Vector3(-4.6f, 0f, 30f), new Color(0.3f, 0.55f, 0.3f)).Configure(20f, 52f, true, 1.3f);
            BuildPerson(new Vector3(4.6f, 0f, 100f), new Color(0.4f, 0.4f, 0.7f)).Configure(92f, 120f, true, 0.7f);
        }

        // ---- Ground ---------------------------------------------------------------

        private void BuildGround()
        {
            // Grass plane, large enough to run beyond the fog so there's no visible edge.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, 0f, 50f);
            ground.transform.localScale = Vector3.one * 32f; // 320 units across
            ground.GetComponent<Renderer>().sharedMaterial =
                Mat(new Color(0.34f, 0.45f, 0.27f), 0f, 0.05f);

            // A bright "start" line so the player has a reference to set off from.
            Box("StartLine", new Vector3(0f, 0.06f, -26f), new Vector3(7f, 0.04f, 0.4f),
                Mat(Color.white, 0f, 0.1f));
        }

        // ---- Car ------------------------------------------------------------------

        private GameObject BuildCar(Vector3 position)
        {
            var car = new GameObject("Car");
            car.transform.position = position;

            // Visuals: a glossy painted lower body, a tinted-glass cabin, and lights.
            var paint = Mat(new Color(0.78f, 0.12f, 0.12f), 0.5f, 0.6f);
            var glass = Mat(new Color(0.08f, 0.1f, 0.13f), 0.2f, 0.9f);
            var headMat = Mat(new Color(1f, 0.97f, 0.85f), 0f, 0.9f, new Color(1f, 0.95f, 0.7f));
            var tailMat = Mat(new Color(0.5f, 0.05f, 0.05f), 0f, 0.9f, new Color(0.7f, 0.05f, 0.05f));

            AddPart(car.transform, "Body", new Vector3(0f, 0.05f, 0f), new Vector3(1.8f, 0.7f, 4.2f), paint);
            AddPart(car.transform, "Cabin", new Vector3(0f, 0.55f, -0.2f), new Vector3(1.6f, 0.6f, 2.0f), glass);
            for (int sx = -1; sx <= 1; sx += 2)
            {
                AddPart(car.transform, "Headlight", new Vector3(0.6f * sx, 0.05f, 2.05f),
                        new Vector3(0.35f, 0.2f, 0.1f), headMat);
                AddPart(car.transform, "Taillight", new Vector3(0.6f * sx, 0.05f, -2.05f),
                        new Vector3(0.35f, 0.2f, 0.1f), tailMat);
            }

            var hull = car.AddComponent<BoxCollider>(); // physics hull (visuals have no colliders)
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

            BuildSteeringWheel(car.transform, controller);
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
            var asphalt = Mat(new Color(0.13f, 0.13f, 0.14f), 0.0f, 0.35f);
            var paintWhite = Mat(new Color(0.92f, 0.92f, 0.92f), 0f, 0.1f);
            var paintYellow = Mat(new Color(0.92f, 0.82f, 0.2f), 0f, 0.1f);

            Box("Road", new Vector3(0f, 0.02f, 50f), new Vector3(7f, 0.04f, 170f), asphalt);

            for (int s = -1; s <= 1; s += 2)
                Box("RoadEdge", new Vector3(3.3f * s, 0.05f, 50f), new Vector3(0.15f, 0.05f, 170f), paintWhite);

            for (int i = 0; i < 28; i++)
                Box("CenterDash", new Vector3(0f, 0.05f, -30f + i * 6f), new Vector3(0.15f, 0.05f, 2f), paintYellow);
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

        private void BuildCrosswalk(CoachRuntime coach)
        {
            const float zCross = 48f;

            // Zebra stripes painted across the road.
            for (int i = 0; i < 6; i++)
            {
                var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "CrosswalkStripe";
                stripe.transform.localScale = new Vector3(0.5f, 0.05f, 2.4f);
                stripe.transform.position = new Vector3(-2.5f + i * 1.0f, 0.05f, zCross);
                stripe.GetComponent<Renderer>().material.color = Color.white;
                Destroy(stripe.GetComponent<BoxCollider>());
            }

            // The crossing pedestrian (visual only — the rule is judged by CrosswalkZone).
            var ped = BuildPerson(new Vector3(-5f, 0f, zCross), new Color(0.2f, 0.4f, 0.85f));
            ped.Configure(-5f, 5f, false, 0f);

            var zoneGo = new GameObject("CrosswalkZone");
            zoneGo.transform.position = new Vector3(0f, 1f, zCross - 1f);
            var box = zoneGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(6f, 3f, 14f);
            var zone = zoneGo.AddComponent<CrosswalkZone>();
            zone.coach = coach;
            zone.pedestrian = ped;
            zone.ruleId = "pedestrian_crosswalk";
        }

        private void BuildIntersection(CoachRuntime coach)
        {
            const float zCross = 78f;

            // Cross street (east-west asphalt) forming a 4-way junction with the main road.
            var cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cross.name = "CrossStreet";
            cross.transform.localScale = new Vector3(70f, 0.04f, 7f);
            cross.transform.position = new Vector3(0f, 0.02f, zCross);
            cross.GetComponent<Renderer>().material.color = new Color(0.12f, 0.12f, 0.13f);
            Destroy(cross.GetComponent<BoxCollider>());

            // A small fleet streaming east across the junction, spaced so gaps appear.
            var fleet = new List<TrafficCar>();
            Color[] carColors =
            {
                new Color(0.25f, 0.5f, 0.85f), new Color(0.85f, 0.8f, 0.2f),
                new Color(0.8f, 0.8f, 0.82f), new Color(0.2f, 0.6f, 0.4f)
            };
            const int n = 4;
            for (int i = 0; i < n; i++)
            {
                var tcRoot = new GameObject($"TrafficCar_{i}");
                AddPart(tcRoot.transform, "Body", new Vector3(0f, 0f, 0f),
                        new Vector3(1.8f, 0.7f, 4.0f), Mat(carColors[i % carColors.Length], 0.4f, 0.6f));
                AddPart(tcRoot.transform, "Cabin", new Vector3(0f, 0.5f, -0.2f),
                        new Vector3(1.6f, 0.5f, 1.9f), Mat(new Color(0.08f, 0.1f, 0.13f), 0.2f, 0.9f));
                var tyre = Mat(new Color(0.05f, 0.05f, 0.06f), 0f, 0.3f);
                for (int wx = -1; wx <= 1; wx += 2)
                    for (int wz = -1; wz <= 1; wz += 2)
                        Cyl(tcRoot.transform, "Wheel", new Vector3(0.9f * wx, -0.3f, 1.3f * wz),
                            new Vector3(0.32f, 0.12f, 0.32f), new Vector3(0f, 0f, 90f), tyre);
                var car = tcRoot.AddComponent<TrafficCar>();
                car.startPoint = new Vector3(-35f, 0.4f, zCross);
                car.endPoint = new Vector3(35f, 0.4f, zCross);
                car.speed = 9f;
                car.startOffset = i / (float)n;
                fleet.Add(car);
            }

            // Yield sign on the approach.
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "YieldSignPole";
            pole.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
            pole.transform.position = new Vector3(2.4f, 1.0f, zCross - 8f);
            pole.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
            Destroy(pole.GetComponent<CapsuleCollider>());

            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "YieldSign";
            sign.transform.localScale = new Vector3(0.7f, 0.7f, 0.08f);
            sign.transform.position = new Vector3(2.4f, 1.9f, zCross - 8f);
            sign.transform.localRotation = Quaternion.Euler(0f, 0f, 45f); // diamond
            sign.GetComponent<Renderer>().material.color = new Color(0.9f, 0.7f, 0.1f);
            Destroy(sign.GetComponent<BoxCollider>());

            // Yield zone covering the approach just south of the cross street.
            var zoneGo = new GameObject("CrossTrafficZone");
            zoneGo.transform.position = new Vector3(0f, 1f, zCross - 6f);
            var box = zoneGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(6f, 3f, 12f); // approach z ~ 66..78
            var zone = zoneGo.AddComponent<CrossTrafficZone>();
            zone.coach = coach;
            zone.traffic = fleet.ToArray();
            zone.conflictCenter = new Vector3(0f, 0.4f, zCross);
            zone.conflictRadius = 9f;
            zone.ruleId = "yield";
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
                // Start the slalom past the stop sign, speed stretch, crosswalk, and junction.
                cone.transform.position = new Vector3(x, 0.5f, 95f + i * coneSpacing);
                cone.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);
                cones.Add(cone);
            }
        }
    }
}
