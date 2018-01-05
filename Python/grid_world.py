'''
@version: 1.0
@author: royran
@contact: iranpeng@gmail.com
@file: grid_world.py
@time: 2018/1/5 16:01
'''
import numpy as np
from unity_environment import UnityEnvironment

if __name__ == '__main__':
    env = UnityEnvironment()
    img = env.reset()
    while True:
        s, r, d = env.step(np.random.randint(0, 4))
        print("Reward: {}, Done: {}".format(r, d))