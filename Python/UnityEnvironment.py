'''
@version: 1.0
@author: royran
@contact: iranpeng@gmail.com
@file: UnityEnvironment.py
@time: 2018/1/2 12:12
'''
import atexit
import socket
import struct
import json
from image_utils import process_pixels
CMD_QUIT  = "QUIT"
CMD_STEP  = "STEP"
CMD_RESET = "RESET"

class UnityEnvironment(object):
    def __init__(self, address="127.0.0.1", port=8008):
        atexit.register(self.close)
        self._socket = None
        self._buffer_size = 10240
        self.port = port
        self._create_socket_server(address, self.port)
        self._listening()

    def _create_socket_server(self, address, port):
        try:
            self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self._socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self._socket.bind((address, port))
        except socket.error:
            self.close()
            raise socket.error("Couldn't launch new environment because worker number {} is still in use. "
                               "You may need to manually close a previously opened environment "
                               "or use a different worker number.".format(self.port))

    def _listening(self):
        try:
            self._socket.listen(1)
            self._conn, _ = self._socket.accept()
        except socket.timeout as e:
            raise socket.error(e.strerror)

    def _recv_bytes(self):
        try:
            data = self._conn.recv(self._buffer_size)
        except Exception as ex:
            raise ex
        return data

    def _recv_image_data(self):
        s = self._recv_bytes()
        data_length = struct.unpack("I", bytearray(s[:4]))[0]
        s = s[4:]
        while len(s) != data_length:
            s += self._recv_bytes()
        return s

    def _recv(self):
        data = self._recv_bytes()
        if data is None:
            return None
        return data.decode('utf-8')

    def _send_bytes(self, bytes_data):
        if bytes_data is None:
            return
        try:
            self._conn.send(bytes_data)
        except Exception as ex:
            raise ex

    def _send(self, msg):
        if msg is None:
            return
        self._send_bytes(msg.encode('utf-8'))

    def reset(self):
        env._send(CMD_RESET)
        img_data = env._recv_bytes()
        # img = process_pixels(img_data)
        # return img
        return None

    def step(self, action):
        env._send(CMD_STEP)
        env._recv_bytes()
        env._send_action(action)
        data = env._recv_image_data()
        img_data = process_pixels(data)
        print("Recv image, shape:", img_data.shape)

    def close(self):
        if self._socket is not None:
            print("Closing...")
            self._send(CMD_QUIT)
            self._socket.close()
            self._socket = None

    def _send_action(self, action):
        action_message = {"Action": action}
        self._conn.send(json.dumps(action_message).encode('utf-8'))

if __name__ == '__main__':
    env = UnityEnvironment()
    env.reset()
    import numpy as np
    while True:
        env.step(np.random.randint(0, 4))
    # env._send(CMD_STEP)
    # env._send_action(1)
    # data = env._recv_bytes()
    # img_data = process_pixels(data)
    # print("Recv image, shape:", img_data.shape)
    # while True:
    #     if flag:
    #         env.send(CMD_RESET)
    #         data = env.recv()
    #         print("Recv:", data)
    #     else:
    #         env.send(CMD_STEP)
    #         data = env.recv_bytes()
    #         img_data = process_pixels(data)
    #         print("Recv image, shape:", img_data.shape)
    #     flag = not flag
