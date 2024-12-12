kubectl apply -f ./src/loki -n monitoring
kubectl apply -f ./src/grafana -n monitoring
kubectl apply -f ./src/tempo -n monitoring
kubectl apply -f ./src/prometheus -n monitoring
kubectl apply -f ./src/collector -n monitoring