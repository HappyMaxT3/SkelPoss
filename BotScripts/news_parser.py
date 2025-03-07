import json
import requests
from bs4 import BeautifulSoup

def parse_tech_news():
    news_items = []
    
    # Пример парсинга с Habr
    try:
        habr_response = requests.get('https://habr.com/ru/flows/develop/')
        habr_soup = BeautifulSoup(habr_response.text, 'html.parser')
        
        for article in habr_soup.select('article.tm-articles-list__item'):
            title = article.select_one('h2.tm-title a').text.strip()
            preview = article.select_one('div.article-formatted-body').text.strip()[:200] + '...'
            news_items.append({
                'title': title,
                'content': preview
            })
            
    except Exception as e:
        print(f"Error parsing Habr: {str(e)}")
    
    return json.dumps(news_items)

if __name__ == "__main__":
    print(parse_tech_news())