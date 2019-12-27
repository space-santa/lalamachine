.PHONY: clean
clean:
	docker stop $$(docker ps -aq); \
	docker system prune --force; \
	docker volume prune --force;

.PHONY: build
build:
	docker build -t tag-reader-service -f Dockerfile . && \
	docker image tag tag-reader-service neptune:5000/tag-reader-service

.PHONY: push
push:
	docker push neptune:5000/tag-reader-service

.PHONY: new-images
new-images: build push