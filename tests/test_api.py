import requests

#проверка наличия тортов
def test_api_returns_cakes(api_url):
    response = requests.get(f"{api_url}/api/cakes")
    assert response.status_code == 200
    cakes = response.json()
    assert isinstance(cakes, list)
    assert len(cakes) > 0  
    assert "name" in cakes[0]
    assert "imageUrl" in cakes[0]

#категории
def test_api_returns_categories(api_url):
    response = requests.get(f"{api_url}/api/categories")
    assert response.status_code == 200
    categories = response.json()
    assert isinstance(categories, list)
    assert len(categories) > 0 
    for category in categories:
        assert "slug" in category
        assert "name" in category
#проверка несуществующего торта
def test_api_cake_not_found(api_url):
    response = requests.get(f"{api_url}/api/cakes/999999")
    assert response.status_code == 404

def test_api_swagger_available(api_url):
    response = requests.get(f"{api_url}/swagger/index.html")
    assert response.status_code == 200
    assert "swagger" in response.text.lower()