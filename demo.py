#!/usr/bin/python
import bluetooth
import struct
import time
import sphero_driver
import sys
#from testfile import calibration

fileName = sys.argv[1]
with open(fileName) as f:
    lines = f.readlines()
velocity = int(lines[0].strip('\n'))
colors = [0,0,0]
route = []
for i in range(1,3):
    colors[i] = int(lines[i].strip('\n'))
for j in range(4,len(lines)):
    route.append(lines[j].strip('\n').split(','))

print("Velocity:" + str(velocity))
print("Color: " + str(colors))
print("route: " + str(route))

#route = [(0.5,0),(0.5,200),(0.5,30)]
sphero = sphero_driver.Sphero()
sphero.connect()
#sphero.set_auto_reconnect(1,0,False)
#sphero.set_timeout2(3600,False)
sphero.set_raw_data_strm(40, 1 , 0, False)
print("Start")
#calibration(sphero)
sphero.start()
#sphero.set_rgb_led(color[0],color[1],color[2],0,False);
for [d,h] in route:
    d = float(d)
    h = int(h)
    print(str(d) + " " + str(h))
    t = d*125.5/velocity
    sphero.roll(velocity,h,1,False)
    print("Time:" + str(t))
    time.sleep(t)
sphero.set_rgb_led(255,0,0,0,False);
time.sleep(5)
sphero.join()
sphero.disconnect()
sys.exit(1)