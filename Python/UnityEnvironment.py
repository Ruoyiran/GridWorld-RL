'''
@version: 1.0
@author: royran
@contact: iranpeng@gmail.com
@file: Server.py
@time: 2018/1/2 12:12
'''
import sys
import atexit
import socket

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
                               "or use a different worker number.".format(str(worker_id)))

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

if __name__ == '__main__':
    env = UnityEnvironment()
    flag = False
    while True:
        if flag:
            env.send(CMD_STEP)
        else:
            env.send(CMD_RESET)
        data = env.recv()
        print("Recv: " + data)
        flag = not flag
