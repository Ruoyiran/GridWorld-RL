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

    load_model = False  # Whether to load a saved model.
    input_size = 84 * 84 * 3
    batch_size = 32  # How many experiences to use for each training step.
    anneling_steps = 100000.  # How many steps of training to reduce startE to endE.
    num_episodes = 1000000  # How many episodes of game environment to train network with.
    pre_train_steps = 50000  # How many steps of random actions before training begins.
    path = "./dqn"  # The path to save our model to.
    h_size = 512  # The size of the final convolutional layer before splitting it into Advantage and Value streams.
    tau = 0.001  # Rate to update target network toward primary network
    total_steps = 0
    start_e = 1  # Starting chance of random action
    end_e = 0.01  # Final chance of random action
    e = start_e
    update_freq = 4
    step_drop_e = (start_e - end_e)/anneling_steps
    experience_buffer = ExperienceBuffer()
    trainables = tf.trainable_variables()
    targetOps = update_target_graph(trainables, tau)
    y = 0.99

    start_lr = 1e-4
    end_lr = 1e-6
    decay_rate = 0.1
    decay_steps = 300000
    learning_rate = tf.placeholder(dtype=tf.float32, name="learning_rate")
    mainQN = DeepQNetwork(h_size, 4, learning_rate)
    targetQN = DeepQNetwork(h_size, 4)
    saver = tf.train.Saver()
    with tf.Session() as sess:
        sess.run(tf.global_variables_initializer())
        update_target(targetOps, sess)
        total_reward = 0
        for i in range(num_episodes+1):
            s = env.reset()
            if total_steps <= pre_train_steps or np.random.rand(1) < e:
                a = np.random.randint(0, 4)
            else:
                a = sess.run(mainQN.predict, feed_dict={mainQN.input_x: [s]})[0]
            s1, r, d = env.step(a)
            total_steps += 1
            total_reward += r
            experience_buffer.add(np.reshape(np.array([s.reshape([input_size]), a, r, s1.reshape([input_size]), d]), [1, 5]))
            if total_steps > pre_train_steps:
                if e > end_e:
                    e -= step_drop_e
                if total_steps % update_freq == 0 and len(experience_buffer.buffer) > batch_size:
                    train_batch = experience_buffer.sample(batch_size)
                    inputs = np.reshape(np.vstack(train_batch[:, 3]), [-1, 84, 84, 3])
                    A = sess.run(mainQN.predict, feed_dict={
                        mainQN.input_x: inputs
                    })
                    Q = sess.run(targetQN.Qout, feed_dict={
                        targetQN.input_x: inputs
                    })
                    doubleQ = Q[range(batch_size), A]
                    targetQ = train_batch[:, 2] + y*doubleQ

                    inputs = np.reshape(np.vstack(train_batch[:, 0]), [-1, 84, 84, 3])
                    exp = (i//decay_steps)
                    lr = max(end_e, start_lr * (decay_rate**exp))
                    _, loss_val = sess.run([mainQN.update_model, mainQN.loss], feed_dict={
                        mainQN.input_x: inputs,
                        mainQN.actions: train_batch[:, 1],
                        mainQN.targetQ: targetQ,
                        learning_rate: lr})
                    update_target(targetOps, sess)
            s = s1
            if d is True:
                print("Step: {}, Total reward: {}, e: {}".format(i, total_reward, e))
                s = env.reset()
                total_reward = 0
            if i % 1000 == 0:
                model_path = "{}/model-{}.ckpt".format(path, i)
                print("===> Step: {}, Saving mode to {}".format(i, model_path))
                saver.save(sess=sess, save_path=model_path)
        export_graph(sess, path, target_nodes="Qout")
        print("Training finished.")