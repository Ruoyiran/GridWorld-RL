"""
@version: 1.0
@author: Roy
@contact: iranpeng@gmail.com
@file: grid_world_training.py
@time: 2018/1/7
"""

import numpy as np
import random
import tensorflow as tf
from unity_environment import UnityEnvironment
from dqn_network import DeepQNetwork
from tensorflow.python.tools import freeze_graph

class ExperienceBuffer(object):
    def __init__(self, buffer_size=50000):
        self.buffer = []
        self.buffer_size = buffer_size

    def add(self, experience):
        if len(self.buffer) + len(experience) >= self.buffer_size:
            clear_old_buffer_count = len(self.buffer) + len(experience) - self.buffer_size
            self.buffer[0:clear_old_buffer_count] = []
        self.buffer.extend(experience)

    def sample(self, size):
        return np.reshape(np.asarray(random.sample(self.buffer, size)), [size, 5])

def update_target_graph(tfVars, tau):
    total_vars = len(tfVars)
    op_hoder = []
    for idx, var in enumerate(tfVars[0:total_vars//2]):
        update_value = tau * var.value() + (1-tau) * tfVars[total_vars//2+idx].value()
        op_hoder.append(tfVars[total_vars//2+idx].assign(update_value))
    return op_hoder

def update_target(op_hoder, sess):
    for op in op_hoder:
        sess.run(op)

def export_graph(sess, model_dir, target_nodes):
    print("Exporting graph...")
    tf.train.write_graph(sess.graph_def, model_dir, 'raw_graph_def.pb', as_text=False)
    ckpt = tf.train.get_checkpoint_state(model_dir)
    output_graph = model_dir + "/graph_def.bytes"
    freeze_graph.freeze_graph(input_graph=model_dir + '/raw_graph_def.pb',
                              input_binary=True,
                              input_checkpoint=ckpt.model_checkpoint_path,
                              output_node_names=target_nodes,
                              output_graph=output_graph,
                              clear_devices=True, initializer_nodes="", input_saver="",
                              restore_op_name="save/restore_all", filename_tensor_name="save/Const:0")

if __name__ == '__main__':
    tf.reset_default_graph()
    env = UnityEnvironment()

    load_model = True  # Whether to load a saved model.
    input_size = 84 * 84 * 3
    h_size = 512
    learning_rate = 0.0001
    mainQN = DeepQNetwork(h_size, 4, learning_rate)
    max_episodes = 50
    total_steps = 0
    pre_train_steps = 10000
    num_episodes = 10000
    anneling_steps = 10000.
    start_e = 1
    end_e = 0.1
    e = start_e
    update_freq = 4
    batch_size = 32
    tau = 0.001
    step_drop_e = (start_e - end_e) / anneling_steps
    y = 0.99
    path = "./dqn"  # The path to save our model to.
    saver = tf.train.Saver()
    with tf.Session() as sess:
        sess.run(tf.global_variables_initializer())
        if load_model is True:
            print('Loading Model...')
            ckpt = tf.train.get_checkpoint_state(path)
            saver.restore(sess, ckpt.model_checkpoint_path)

        for i in range(num_episodes + 1):
            episode_buffer = ExperienceBuffer()
            total_reward = 0
            j = 0
            s = env.reset()
            while j < max_episodes:
                j += 1
                a = sess.run(mainQN.predict, feed_dict={mainQN.input_x: [s]})[0]
                s1, r, d = env.step(a)
                total_steps += 1
                total_reward += r
                s = s1
                if d is True:
                    break
