//--- Aura Script -----------------------------------------------------------
// Grocery Shops
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class GroceryShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Grocery", 50004);      	// Bread
		Add("Grocery", 50002);      	// Slice of Cheese
		Add("Grocery", 50220);		// Corn Powder
		Add("Grocery", 50206);		// Chocolate
		Add("Grocery", 50217);		// Celery
		Add("Grocery", 50219);		// Basil
		Add("Grocery", 50219, 5);	// Basil x5
		Add("Grocery", 50219, 10);	// Basil x10
		Add("Grocery", 50219, 20);	// Basil x20
		Add("Grocery", 50218);		// Tomato
		Add("Grocery", 50218, 5);	// Tomato x5
		Add("Grocery", 50218, 10);	// Tomato x10
		Add("Grocery", 50218, 20);	// Tomato x20
		Add("Grocery", 50045);		// Pine Nut
		Add("Grocery", 50047);		// Camellia Seeds	
		Add("Grocery", 50018);		// Baking Chocolate
		Add("Grocery", 50018, 5);	// Baking Chocolate x5
		Add("Grocery", 50018, 10);	// Baking Chocolate x10
		Add("Grocery", 50018, 20);	// Baking Chocolate x20	
		Add("Grocery", 50111);		// Carrot
		Add("Grocery", 50111, 5);	// Carrot x5
		Add("Grocery", 50111, 10); 	// Carrot x10
		Add("Grocery", 50111, 20); 	// Carrot x20
		Add("Grocery", 50114);		// Garlic
		Add("Grocery", 50114, 5);	// Garlic x5
		Add("Grocery", 50114, 10);	// Garlic x10
		Add("Grocery", 50114, 20);	// Garlic x20
		Add("Grocery", 50127);		// Shrimp
		Add("Grocery", 50127, 5);	// Shrimp x5
		Add("Grocery", 50127, 10);	// Shrimp x10
		Add("Grocery", 50127, 20);	// Shrimp x20
		Add("Grocery", 50131);      	// Sugar
		Add("Grocery", 50131, 5);	// Sugar x5
		Add("Grocery", 50131, 10);  	// Sugar x10
		Add("Grocery", 50131, 20);	// Sugar x20
		Add("Grocery", 50132);      	// Salt
		Add("Grocery", 50132, 5);	// Salt x5
		Add("Grocery", 50132, 10);  	// Salt x10
		Add("Grocery", 50132, 20);	// Salt x20
		Add("Grocery", 50148);      	// Yeast
		Add("Grocery", 50148, 5);	// Yeast x5
		Add("Grocery", 50148, 10);  	// Yeast x10
		Add("Grocery", 50148, 20);	// Yeast x20
		Add("Grocery", 50153);      	// Deep Frying batter
		Add("Grocery", 50153, 5);	// Deep Frying batter x5
		Add("Grocery", 50153, 10); 	// Deep Frying batter x10
		Add("Grocery", 50153, 20);	// Deep Frying batter x20
		Add("Grocery", 50156);      	// Pepper
		Add("Grocery", 50156, 5);	// Pepper x5
		Add("Grocery", 50156, 10);  	// Pepper x10
		Add("Grocery", 50156, 20);	// Pepper x20
		Add("Grocery", 50046);		// Juniper Berry		
		Add("Grocery", 50112);      	// Strawberry
		Add("Grocery", 50112, 5);	// Strawberry x5
		Add("Grocery", 50112, 10);  	// Strawberry x10
		Add("Grocery", 50112, 20);	// Strawberry x20
		Add("Grocery", 50121);      	// Butter
		Add("Grocery", 50121, 5);	// Butter x5
		Add("Grocery", 50121, 10);  	// Butter x10
		Add("Grocery", 50121, 20);	// Butter x20
		Add("Grocery", 50142);      	// Onion
		Add("Grocery", 50142, 5);	// Onion x5
		Add("Grocery", 50142, 10);  	// Onion x10
		Add("Grocery", 50142, 20);	// Onion x20
		Add("Grocery", 50108);		// Chicken Wings x
		Add("Grocery", 50108, 5);	// Chicken Wings x5
		Add("Grocery", 50108, 10);	// Chicken Wings x10
		Add("Grocery", 50108, 20);	// Chicken Wings x20
		Add("Grocery", 50130);		// Whipped Cream
		Add("Grocery", 50130, 5);	// Whipped Cream x5
		Add("Grocery", 50130, 10);	// Whipped Cream x10
		Add("Grocery", 50130, 20);	// Whipped Cream x20
		Add("Grocery", 50185);		// Curry Powder
		Add("Grocery", 50185, 5);	// Curry Powder x5
		Add("Grocery", 50185, 10);	// Curry Powder x10
		Add("Grocery", 50185, 20);	// Curry Powder x20
		Add("Grocery", 50186);		// Red Pepper Powder
		Add("Grocery", 50186, 5);	// Red Pepper Powder x5
		Add("Grocery", 50186, 10);	// Red Pepper Powder x10
		Add("Grocery", 50186, 20);	// Red Pepper Powder x20
		Add("Grocery", 50005);      	// Large Meat
		Add("Grocery", 50001);      	// Big Lump of Cheese
		Add("Grocery", 50135);		// Rice
		Add("Grocery", 50135, 5);	// Rice x5
		Add("Grocery", 50135, 10);	// Rice x10
		Add("Grocery", 50135, 20);	// Rice x20
		Add("Grocery", 50138);		// Cabbage
		Add("Grocery", 50138, 5);	// Cabbage x5
		Add("Grocery", 50138, 10);	// Cabbage x10
		Add("Grocery", 50138, 20);	// Cabbage x20
		Add("Grocery", 50139);		// Button Mushroom
		Add("Grocery", 50139, 5);	// Button Mushroom x5
		Add("Grocery", 50139, 10);	// Button Mushroom x10
		Add("Grocery", 50139, 20);	// Button Mushroom x20
		Add("Grocery", 50145);      	// Olive Oil
		Add("Grocery", 50145, 5);	// Olive Oil x5
		Add("Grocery", 50145, 10);  	// Olive Oil x10
		Add("Grocery", 50145, 20);	// Olive Oil x20
		Add("Grocery", 50187);		// Lemon
		Add("Grocery", 50187, 5);	// Lemon x5
		Add("Grocery", 50187, 10);	// Lemon x10
		Add("Grocery", 50187, 20);	// Lemon x20
		Add("Grocery", 50188);		// Orange 
		Add("Grocery", 50188, 5);	// Orange x5
		Add("Grocery", 50188, 10);	// Orange x10
		Add("Grocery", 50188, 20);	// Orange x20
		Add("Grocery", 50189);		// Thyme 
		Add("Grocery", 50189, 5);	// Thyme x5
		Add("Grocery", 50189, 10);	// Thyme x10
		Add("Grocery", 50189, 20);	// Thyme x20
		Add("Grocery", 50421);		// Pecan
		Add("Grocery", 50421, 5);	// Pecan x5
		Add("Grocery", 50421, 10);	// Pecan x10
		Add("Grocery", 50421, 20);	// Pecan x20
		Add("Grocery", 50426);		// Peanuts
		Add("Grocery", 50426, 5);	// Peanuts x5
		Add("Grocery", 50426, 10);	// Peanuts x10
		Add("Grocery", 50426, 20);	// Peanuts x20
		Add("Grocery", 50123);		// Roasted Bacon
		Add("Grocery", 50134);		// Sliced bread
		Add("Grocery", 50133);		// Beef
		Add("Grocery", 50122);		// Bacon
		Add("Grocery", 50122, 5);	// Bacon x5
		Add("Grocery", 50122, 10);	// Bacon x10
		Add("Grocery", 50122, 20);	// Bacon x20
		Add("Grocery", 50430);		// Grapes
		Add("Grocery", 50430, 5);	// Grapes x5
		Add("Grocery", 50430, 10);	// Grapes x10
		Add("Grocery", 50430, 20);	// Grapes x20
		Add("Grocery", 50120);		// Steamed Rice
		Add("Grocery", 50102);		// Potato Salad
		Add("Grocery", 50431);		// Pumpkin
		Add("Grocery", 50431, 5);	// Pumpkin x5
		Add("Grocery", 50431, 10);	// Pumpkin x10
		Add("Grocery", 50431, 20);	// Pumpkin x20
		Add("Grocery", 50006);		// Slice of meat
		Add("Grocery", 50006, 5);   	// Slice of Meat x5
		Add("Grocery", 50006, 10);	// Slice of Meat x10
		Add("Grocery", 50006, 20);	// Slice of Meat x20
		Add("Grocery", 50104);      	// Egg Salad
		Add("Grocery", 50101);		// Potato Egg Salad

		Add("Gift", 52010);         	// Ramen
		Add("Gift", 52021);         	// Slice of Cake
		Add("Gift", 52019);         	// Heart Cake
		Add("Gift", 52022);         	// Wine
		Add("Gift", 52023);         	// Wild Ginseng
	}
}
