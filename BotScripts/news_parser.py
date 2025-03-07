import json
import sys
import requests
from bs4 import BeautifulSoup
from unicodedata import normalize
from urllib.parse import urljoin

sys.stdout.reconfigure(encoding='utf-8')

def parse_tech_news():
    news_items = []
    try:
        base_url = 'https://habr.com'
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36'
        }
        response = requests.get(
            'https://habr.com/ru/flows/develop/',
            headers=headers,
            timeout=10
        )
        response.encoding = 'utf-8'
        
        soup = BeautifulSoup(response.text, 'html.parser')
        
        for article in soup.select('article.tm-articles-list__item'):
            try:
                title_elem = article.select_one('h2.tm-title a')
                content_elem = article.select_one('div.tm-article-body')
                link_elem = title_elem['href'] if title_elem else None
                
                title = normalize('NFC', title_elem.text.strip()) if title_elem else "Без заголовка"
                content = normalize('NFC', content_elem.text.strip()[:800] + '...') if content_elem else "Нет содержимого"
                full_url = urljoin(base_url, link_elem) if link_elem else "#"
                
                news_items.append({
                    'title': title,
                    'content': content,
                    'url': full_url 
                })
                
            except Exception as e:
                print(f"Ошибка в статье: {str(e)}", file=sys.stderr)
                continue

        return json.dumps(news_items, ensure_ascii=False)

    except Exception as e:
        return json.dumps({'error': str(e)})

if __name__ == "__main__":
    try:
        print(parse_tech_news())
    except Exception as e:
        print(json.dumps({'error': f"Critical error: {str(e)}"}))