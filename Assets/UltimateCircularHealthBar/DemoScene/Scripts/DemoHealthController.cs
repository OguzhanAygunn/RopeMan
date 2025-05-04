using UnityEngine;
using Random = UnityEngine.Random;

//Be sure to include the namespace RengeGames.HealthBars at the top of your scripts when using the health bars.
using RengeGames.HealthBars;

namespace RengeGames.HealthBars.Demo {

	public class DemoHealthController : MonoBehaviour {

		// The parent name and the health bar name are used to identify the health bar you want to modify.
		// they are only used by the StatusBarsManager class, so if you don't want to use that class, you can ignore these.
		// they can be changed for every health bar in the inspector or in code 
		// <healthBar>.ParentName = "NewParentName";
		// <healthBar>.Name = "NewHealthBarName";
		public string parentName = "Player";
		public string healthBarName = "Primary";


		// these are just some example properties that can be changed in the inspector.
		public bool updateFromScript = true;
		public float value = 0.5f;



		// This is an example of how to get a reference to a health bar from a script.
		// This is the component name for the health bar.
		// Be sure to include the namespace RengeGames.HealthBars at the top of your own script.
		public RadialSegmentedHealthBar exampleHealthBar;

		void Start() {
			// Here are some examples of changing health bar properties directly.
			// Each porperty has a tooltip that explains what it does.
			exampleHealthBar.InnerColor.Value = new Color(1,1,1,1); // set the color of the value. Hover over the property to see more information on it
			exampleHealthBar.RemoveSegments.Value = 0; // Set the value by removing segments.
			exampleHealthBar.SetPercent(1); // Set the value by percent.
		}

		private void Update() {
			if (updateFromScript) {
				// The StatusBarsManager class offers an easy way to modify health bars from anywhere without needing to get a reference to them.
				StatusBarsManager.SetPercent(parentName, healthBarName, value);
			}
		}

		public void SetHealthPercent(float value) {
			// Setting the percent of "Player", "Main" changes all health bar values with the parent name "Player" and the health bar name "Main"
			// give health bars different names to change them individually
			StatusBarsManager.SetPercent("Player", "Main", value);

			// you can also use combos like "Player2" "Main1" and "Player2" "Main2" to have more control.
			// this changes "Main1" and "Main2" because they have the same parent name "Player2"
			StatusBarsManager.SetPercent("Player2", value);
			// this changes each health bar individually
			StatusBarsManager.SetPercent("Player2", "Main1", value + 0.25f);
			StatusBarsManager.SetPercent("Player2", "Main2", value - 0.25f);

		}

		public void ToggleNoise(bool toggle) {
			// properties can also be changed using the manager class.
			StatusBarsManager.SetShaderPropertyValue(parentName, healthBarName, RadialHealthBarProperties.EmptyNoiseEnabled, toggle);

			//if you want to manually store the health bars in an array, this is how you can access properties
			//you can select specific healthbars by querying for parent names and health bar names
			//if you don't want to use the health bar manager at all, you can disable it completely by calling StatusBarsManager.Disable();
			//myCustomHealthBarsArray?.ForEach((healthBar) => { healthBar.EmptyNoiseEnabled.Value = toggle; });
		}

		public void Instantiate() {
			// This is an example of how to instantiate a health bar from a script.
			// Of course there are many ways to do this.
			var parent = new GameObject("Daddy o' 100");
			parent.transform.position = new Vector3(0, 0, 0);
			for (int i = 0; i < 100; i++) {
				var go = new GameObject("HealthBar" + i, typeof(RadialSegmentedHealthBar));
				go.transform.parent = parent.transform;
				go.transform.position = new Vector3((Random.value * 2 - 1) * 6, (Random.value * 2 - 1) * 3, 0);
			}
		}
	}
}