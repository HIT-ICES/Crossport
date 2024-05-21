tc qdisc show dev enp5s0 # 确认配置
tc filter show dev enp5s0 # 确认配置
# Clear
tc qdisc delete dev enp5s0 parent 1:1 handle 10:
tc qdisc delete dev enp5s0 root handle 1:
tc filter del dev enp5s0 parent 1: protocol ip pref 2 u32
tc filter del dev enp5s0 parent 1: protocol ip pref 1 u32
tc qdisc show dev enp5s0 # 确认配置
tc filter show dev enp5s0 # 确认配置