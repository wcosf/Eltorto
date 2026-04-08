import pytest
import requests
import psycopg2
import time
import os

@pytest.fixture(scope="session")
def docker_compose_file(pytestconfig):
    return str(pytestconfig.rootdir / "infra" / "docker-compose.yml")

@pytest.fixture(scope="session")
def docker_compose_project_name():
    return "eltorto"

@pytest.fixture(scope="session")
def docker_setup():
    import subprocess
    with open("infra/.env.test", "w") as f:
        f.write("DB_PASSWORD=test\n")
    yield
    # Чистка
    os.remove("infra/.env.test")
#проверка ответа API
@pytest.fixture(scope="session")
def api_url(docker_ip, docker_services, docker_setup):
    port = docker_services.port_for("api", 8080)
    url = f"http://{docker_ip}:{port}"
    
    def is_api_ready():
        try:
            response = requests.get(f"{url}/api/cakes", timeout=5)
            return response.status_code == 200
        except:
            return False
    
    docker_services.wait_until_responsive(
        timeout=120.0, pause=2.0, check=is_api_ready
    )
    return url
#база данных
@pytest.fixture(scope="session")
def db_connection(docker_ip, docker_services, docker_setup):
    port = docker_services.port_for("postgres", 5432)
    
    def is_db_ready():
        try:
            conn = psycopg2.connect(
                host=docker_ip,
                port=port,
                user="postgres",
                password="test",
                database="eltorto_pg",
                connect_timeout=5
            )
            conn.close()
            return True
        except Exception as e:
            return False
    
    docker_services.wait_until_responsive(
        timeout=120.0, pause=2.0, check=is_db_ready
    )
    
    conn = psycopg2.connect(
        host=docker_ip,
        port=port,
        user="postgres",
        password="test",
        database="eltorto_pg"
    )
    yield conn
    conn.close()
#проверка фронтенда
@pytest.fixture(scope="session")
def frontend_url(docker_ip, docker_services, docker_setup):
    port = docker_services.port_for("frontend", 80)
    url = f"http://{docker_ip}:{port}"
    
    def is_frontend_ready():
        try:
            response = requests.get(url, timeout=5)
            return response.status_code == 200
        except:
            return False
    
    docker_services.wait_until_responsive(
        timeout=120.0, pause=2.0, check=is_frontend_ready
    )
    return url
