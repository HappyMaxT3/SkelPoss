import random

import undetected_chromedriver as uc
import time
from bs4 import BeautifulSoup
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import re
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options
from undetected_chromedriver import ChromeOptions
OPOSSUM = """                                                 
                  :+X$XXXxx+x+;+;.                                             
             .xXX$$$&$$$$&&$$$X$&$&$X$XXx;                                       
       .:X$&&&$XXX$&$&&&&&&&&&xx&&&&$xX$$x+x$+                                   
    .+XX+:x&&;xxx$XX&&$&$X$$&$$XX$&&&$$$$&$$$XxXx:                               
    .xx:.......x&&&&&&&&&&&&&&&&$&&&&&&&&$$$XXxX&&$$.                            
    .+;.:;+...;x&&&&&&&&&&&&&&&&&&&&&&&&&&&x       :++:                          
     +.;$x  ;+:&&&&&&&&X          &&&&&&&&&$            .:.                      
     .......  &&&&&X.&&.           ;&&&  x&&&&               ...                 
     .:. :;&&&X.                    .       &&                        .......                                 
"""
def get_options():
    options = webdriver.ChromeOptions()
    options.add_argument("--user-agent=*")  # Пример User-Agent
    options.add_argument("--lang=ru")  # Укажите язык
    options.add_argument("--disable-extensions")  # Отключить расширения
    options.add_argument("--disable-notifications")  # Отключить уведомления
    options.add_argument("--disable-blink-features=AutomationControlled")
    return options
def get_page_links_enhanced(stop_link: str,max_pages: int)-> set:
    """Получает все ссылки на товары с DNS"""
    links = set()
    print("Opossum starts his crawl....")
    print(OPOSSUM)
    driver = uc.Chrome(options=get_options(), use_subprocess=True)
    # ----------Менять каталог для поиска тут-----
    driver.get("https://www.dns-shop.ru/novelties/?stock=now-today-tomorrow-later-out_of_stock&p=1")
    amount = WebDriverWait(driver, 60).until(
        EC.presence_of_all_elements_located((By.CSS_SELECTOR, 'li[data-role="pagination-page"]'))
    )
    amount = int(amount[-1].get_attribute("data-page-number"))
    amount = min(amount,max_pages)

    print(f'pages to parse: {amount}')
    for i in range(amount):
        print(f'pages left: {amount -1 -i}')
        time.sleep(random.randint(1,10))
        #----------Менять каталог для поиска тут-----
        driver.get(f"https://www.dns-shop.ru/novelties/?stock=now-today-tomorrow-later-out_of_stock&p={i+1}")
        elements = WebDriverWait(driver, 60).until(
            EC.presence_of_all_elements_located((By.XPATH, '//a[@class="catalog-product__name ui-link ui-link_black"]'))
        )
        for element in elements:
            try:
                # Ожидание, пока конкретный элемент станет кликабельным
                clickable_element = WebDriverWait(driver, 60).until(
                    EC.element_to_be_clickable(element)
                )
                # Получение ссылки
                link = clickable_element.get_attribute("href")
                if link == stop_link:
                    print(f"reached stop link: {stop_link}")
                    return links
                links.add(link + 'characteristics/')
            except Exception as e:
                print(f"Элемент не стал кликабельным: {e}")
    driver.quit()
    print("parsing links complete.")
    return links

def get_product_description_page(prod_url: str)->str:
    """Получает описание со страницы товара по ссылке на него в DNS"""
    driver = uc.Chrome(options=get_options(), use_subprocess=True)
    driver.get(prod_url)
    print(f'processing {prod_url}')
    element = WebDriverWait(driver, 60).until(
        EC.presence_of_element_located((By.XPATH, '//div[@class="product-card-description__main"]'))
    )
    time.sleep(random.randint(1,10))
    button = element.find_element(By.TAG_NAME, 'button')
    time.sleep(random.randint(1,10))
    button.click()
    print("waiting for content to load...")
    time.sleep(random.randint(1,10))
    element = WebDriverWait(driver, 60).until(
        EC.presence_of_element_located((By.XPATH, '//div[@class="product-card-description__main"]'))
    )
    time.sleep(random.randint(1,10))
    #driver.quit() должен вызываться после всех операций с элементами страницы, иначе это вызовет ошибку и все сломается
    page = element.get_attribute("innerHTML").encode('utf-8', errors='ignore').decode('utf-8')
    driver.quit()
    return page

def parse_product_description(desc_to_parse:str, source:str="NONE")->str:
    """Берет скачанную страницу с DNS и парсит ее в краткий и удобный формат чтоб запихать в RAG"""
    product_data = ""
    soup = BeautifulSoup(desc_to_parse, 'html.parser')
    desc = soup.find('div', class_="product-card-description-text")
    product_data += "Описание товара:\n" + re.sub(r'\n|\r|\t', '', desc.text) + "\n"
    # тут по пунктам ищется название группы хар-к, хар-ка : значение.
    specs = soup.find('div', class_="product-characteristics")
    groups = specs.find_all('div', class_="product-characteristics__group")
    # эта строка дублирует все, это типо 2 css класса в 1 поле.
    # groups = groups +  specs.find_all('div', class_="product-characteristics__group product-characteristics__ovh")
    type = "NONE"
    model = "NONE"
    for group in groups:
        title = group.find('div', class_="product-characteristics__group-title").text
        spec_list = group.find_all('li', class_="product-characteristics__spec")
        product_data += f'{title}:\n'
        #print(title)
        for spec in spec_list:
            cleaned_name = re.sub(r'\n|\r|\t', '',
                                  spec.find('span', class_='product-characteristics__spec-title-content').text)
            cleaned_value = re.sub(r'\n|\r|\t', '',
                                   spec.find('div', class_='product-characteristics__spec-value').text)
            product_data += f'{cleaned_name}: {cleaned_value}' + ";"
            if cleaned_name == "Тип":
                type = cleaned_value
            elif cleaned_name == "Модель":
                model = cleaned_value
        product_data += "\n"
    return source +"\n" + type+"\n" +model+"\n" +product_data
if __name__ == "__main__":
    stop_link = "https://www.dns-shop.ru/product/c18aecca92a32cc7/215-monitor-iiyama-prolite-xu2293hsu-b6-cernyj/"
    links = get_page_links_enhanced(stop_link,1)
    print(links)
    print(len(links))
    for link in links:
        desc_page = get_product_description_page(link)
        with open(f"{link.split('/')[-3]}.txt",'w',encoding="utf-8",errors='ignore') as f:
            f.write(parse_product_description(desc_page,link))