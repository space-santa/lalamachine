.PHONY: qmlformat
qmlformat:
	qmlformat -i assets/qml/*.qml

.PHONY: qmllint
qmllint:
	qmllint assets/qml/*.qml

.PHONY: run
run:
	python main.py

.PHONY: isort
isort:
	isort --skip .venv src

.PHONY: check-isort
check-isort:
	isort --diff --skip .venv src

.PHONY: black
black:
	black --exclude migrations src

.PHONY: check-black
check-black:
	black --exclude migrations --check src

.PHONY: pylint
pylint:
	pylint src

.PHONY: flake8
flake8:
	flake8 src --count --show-source --statistics

.PHONY: test
test:
	pytest

.PHONY: check-all
check-all: fix flake8 pylint test qmllint

.PHONY: fix
fix: isort black qmlformat
