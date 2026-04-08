import requests
#доступность фронтенда
def test_frontend_returns_html(frontend_url):
    response = requests.get(frontend_url)
    assert response.status_code == 200
    assert "<!doctype html>" in response.text.lower()
    assert "<app-root>" in response.text
#CSS файлы загружаются
def test_frontend_has_css(frontend_url):
    html_response = requests.get(frontend_url)
    import re
    css_pattern = r'href="([^"]+\.css)"'
    css_files = re.findall(css_pattern, html_response.text)
    assert len(css_files) > 0, "No CSS files found"
    
    css_url = css_files[0]
    if css_url.startswith('/'):
        css_url = frontend_url + css_url
    response = requests.get(css_url)
    assert response.status_code == 200