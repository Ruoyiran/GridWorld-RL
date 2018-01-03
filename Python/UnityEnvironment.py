'''
@version: 1.0
@author: royran
@contact: iranpeng@gmail.com
@file: UnityEnvironment.py
@time: 2018/1/2 12:12
'''
import sys
import atexit
import socket
import struct
from PIL import Image
import io
import numpy as np
import cv2 as cv

CMD_EXIT  = "EXIT"
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

    def recv(self):
        try:
            data = self._conn.recv(self._buffer_size).decode('utf-8')
        except Exception as e:
            sys.exit()
        return data

    def send(self, msg):
        if msg is None:
            return
        try:
            self._conn.send(msg.encode('utf-8'))
        except Exception:
            pass

    def close(self):
        if self._socket is not None:
            print("Closing...")
            self.send(CMD_EXIT)
            self._socket.close()
            self._socket = None

    def recv_bytes(self):
        try:
            s = self._conn.recv(self._buffer_size)
            data_length = struct.unpack("I", bytearray(s[:4]))[0]
            s = s[4:]
            while len(s) != data_length:
                s += self._conn.recv(self._buffer_size)
        except Exception as ex:
            raise ex
        return s

    def send_action(self):
        """
        Send dictionary of actions, memories, and value estimates over socket.
        :param action: a dictionary of lists of actions.
        :param memory: a dictionary of lists of of memories.
        :param value: a dictionary of lists of of value estimates.
        """
        try:
            self._conn.recv(self._buffer_size)
        except Exception as ex:
            raise ex
        # action_message = {"action": action, "memory": memory, "value": value}
        self._conn.send("1".encode('utf-8'))

def process_pixels(image_bytes=None):
    s = bytearray(image_bytes)
    image = Image.open(io.BytesIO(s))
    return np.array(image)

if __name__ == '__main__':
    env = UnityEnvironment()
    flag = False
    env.send(CMD_STEP)
    env.send_action()
    data = env.recv_bytes()
    img_data = process_pixels(data)
    print("Recv image, shape:", img_data.shape)
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
