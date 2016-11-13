from msvcrt import getch
import bluetooth
import struct
import time
import sphero_driver
import sys
	

sphero = sphero_driver.Sphero()
sphero.connect()
print("Start Calibration")
sphero.start()
time.sleep(0.5)
sphero.set_back_led(200,False)
#sphero.set_rgb_led(0,255,0,0,False)
heading = 0
print("Start getting Input")
while True:
	key = ord(getch())
	if key == 13: #Enter
		print("Done Calibration")
		break
	elif key == 224: #Special keys (arrows, f keys, ins, del, etc.)
		key = ord(getch())
		if key == 75: #Left arrow
			print("Left!")
			time.sleep(0.03)
			heading = heading -20
			if heading < 0:
				heading = heading + 360
			sphero.roll(0,heading,1,False)
		elif key == 77: #Right arrow
			print("Right!")
			heading = 20 + heading
			if heading > 360:
				heading = heading - 360
			time.sleep(0.03)
			sphero.roll(0,heading,1,False)
sphero.set_heading(heading, False)
sphero.join()
sphero.disconnect()
sys.exit(1)