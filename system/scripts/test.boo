import System.Collections
import Aura.Channel.Network
import Aura.Channel.Scripting.Scripts
import Aura.Channel.World.Entities
import Aura.Shared.Util

class Npc1Script(NpcScript):
	def Load():
		SetName("_morrighan")
		SetLocation(1, 12950, 38219, 0)

	def Talk(c as Creature):
		Msg(c, "test 2")
		#Close(c)

		Msg(c, "test 2")
		#Return()
		yield
