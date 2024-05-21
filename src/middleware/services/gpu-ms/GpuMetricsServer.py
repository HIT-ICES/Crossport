import pynvml
import sys
from gpustat.core import GPUStatCollection
import json
from flask import Flask, request

app = Flask(__name__)

@app.route('/gpu_stats', methods=['GET'])
def get_gpu_stats():
    pids=set()
    gpu_stats = GPUStatCollection.new_query(debug=False, id=None)
    pynvml.nvmlShutdown()
    return gpu_stats.jsonify()

if __name__ == '__main__':
    port=int( sys.argv[1]) if len(sys.argv)>1 else 20240
    app.run(host='0.0.0.0',port=port)