tc qdisc show dev enp5s0 # 确认配置
tc filter show dev enp5s0 # 确认配置
# Apply
tc qdisc add dev enp5s0 root handle 1: prio # 创建一个队列规则
tc qdisc add dev enp5s0 parent 1:1 handle 10: netem delay 131ms 26ms # 添加延迟 42+-6ms
tc filter add dev enp5s0 protocol ip parent 1: prio 1 u32 match ip src 10.123.0.0/16 match ip dst 10.123.3.3 flowid 1:1 # 匹配源 IP 地址为 10.123.0.0/16 的流量
tc filter add dev enp5s0 protocol ip parent 1: prio 1 u32 match ip src 10.123.3.3 match ip dst 10.123.0.0/16 flowid 1:1 # 匹配目标 IP 地址为 10.123.0.0/16 的流量

tc qdisc show dev enp5s0 # 确认配置
tc filter show dev enp5s0 # 确认配置
