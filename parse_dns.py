import random
import concurrent.futures
from itertools import product

import undetected_chromedriver as uc
import time
from bs4 import BeautifulSoup
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import re
import threading
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options
from undetected_chromedriver import ChromeOptions
from concurrent.futures import ThreadPoolExecutor
import numpy as np

driver_lock = threading.Lock()

links_left = 0

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
OPOSSING = """
                        ...:;;;++++++++++;;;;;;;;;;;;;;;
                  ...:;++++;;;;;;;;;;;;;;++x++xxxxxXXXXX
              ...:::::.......::::.........::..::::;+x+++
      ........:::::::::::....::::::.....:;++xXX$XXX$$x++
.....:::::::::::;;;;+X$$Xx+;::.:::::::::::;++xxxXXXx++;;
:x+++;;;;;;;;;;::::::..............:::.........:+xx+++;;
 ..;;;;;+++++;;;;;;;;;;::;;;;;;::::::.:::::::::;++++x+++
          ....         ..;XXXXXXXX+;;;;;;;;;;++++++++;;;
                         .:XXX$Xxxx+xXXxxx+++++++++++;;;
                        ..;xxXx++x+::;+xxx++++++++++++xX
                        ..:;;;;;;..  .:;+xxX$XXXXXXXXXXX
"""


def get_options(is_headless=False):
    options = ChromeOptions()
    options.add_argument("--disable-gpu")
    options.add_argument("--no-sandbox")
    options.add_argument(f"--remote-debugging-port={9222 + threading.get_ident() % 4000}")
    if is_headless:
        options.add_argument("--headless")
    return options

def get_page_links_enhanced(driver,stop_link: str,max_pages: int)-> set:
    """Получает все ссылки на товары с DNS"""
    parsed_links = set()
    print("Opossum starts his crawl....")
    print(OPOSSUM)

    # ----------Менять каталог для поиска тут-----
    driver.get("https://www.dns-shop.ru/catalog/17a8d33716404e77/ventilyatory/?order=6&stock=now-today-tomorrow-later-out_of_stock&rating=1&brand=brayer-electrolux-reoka-xiaomi-aresa-ballu-blackdecker-deerma-delta-domie-dux-eurostek-first-galaxyline-hiper-hitt-homieland-kitfort-kubic-lex-lumme-makita-marta-midea-mijia-mystery-oasis-polaris-primera-proficare-qualitell-redmond-rix-scarlett-sencor-smartmi-solerpalau-solove-sonnen-stadlerform-stingray-tefal-tendenza-timberk-vitek-zanussi-zdk-zmi-airline-beko-goldstar-maunfeld-neoclima-obsidian-starwind-steba-taurus&p=1")
    amount = WebDriverWait(driver, 100).until(
        EC.presence_of_all_elements_located((By.CSS_SELECTOR, 'li[data-role="pagination-page"]'))
    )
    amount = int(amount[-1].get_attribute("data-page-number"))
    amount = min(amount,max_pages)

    print(f'pages to parse: {amount}')
    for i in range(amount):
        print(f'pages left: {amount -i}')
        time.sleep(random.randint(1,10))
        #----------Менять каталог для поиска тут-----
        driver.get(f"https://www.dns-shop.ru/catalog/17a8d33716404e77/ventilyatory/?order=6&stock=now-today-tomorrow-later-out_of_stock&rating=1&brand=brayer-electrolux-reoka-xiaomi-aresa-ballu-blackdecker-deerma-delta-domie-dux-eurostek-first-galaxyline-hiper-hitt-homieland-kitfort-kubic-lex-lumme-makita-marta-midea-mijia-mystery-oasis-polaris-primera-proficare-qualitell-redmond-rix-scarlett-sencor-smartmi-solerpalau-solove-sonnen-stadlerform-stingray-tefal-tendenza-timberk-vitek-zanussi-zdk-zmi-airline-beko-goldstar-maunfeld-neoclima-obsidian-starwind-steba-taurus&p={i+1}")
        elements = WebDriverWait(driver, 100).until(
            EC.presence_of_all_elements_located((By.XPATH, '//a[@class="catalog-product__name ui-link ui-link_black"]'))
        )
        for element in elements:
            try:
                # Ожидание, пока конкретный элемент станет кликабельным
                clickable_element = WebDriverWait(driver, 100).until(
                    EC.element_to_be_clickable(element)
                )
                # Получение ссылки
                link = clickable_element.get_attribute("href")
                if link == stop_link:
                    print(f"reached stop link: {stop_link}")
                    return parsed_links
                parsed_links.add(link + 'characteristics/')
            except Exception as e:
                print(f"Элемент не стал кликабельным: {e}")
    print("parsing links complete.")
    return parsed_links

def get_product_description_page(driver, prod_url: str)->str:
    """Получает описание со страницы товара по ссылке на него в DNS"""
    driver.get(prod_url)
    print(f'processing {prod_url}')
    element = WebDriverWait(driver, 100).until(
        EC.presence_of_element_located((By.XPATH, '//div[@class="product-card-description__main"]'))
    )
    time.sleep(random.randint(1,10))
    button = element.find_element(By.TAG_NAME, 'button')
    time.sleep(random.randint(1,10))
    action = webdriver.ActionChains(driver)
    for i in range(random.randint(0,20)):
        action.move_by_offset(random.randint(10,30),random.randint(10,40))
        action.pause(1/random.randint(1,30))
    action.perform()
    button.click()
    #print("waiting for content to load...")
    time.sleep(random.randint(1,10))
    time.sleep(random.randint(1,10))
    #driver.quit() должен вызываться после всех операций с элементами страницы, иначе это вызовет ошибку и все сломается
    page = driver.page_source.encode('utf-8', errors='ignore').decode('utf-8')
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
    return "Источник: "+source +"\n" + "Тип: " +type+"\n" +"Модель: "+model+"\n"+product_data


def process_product(link: str):
    filename = f"{link.split('/')[-3]}.txt"

    global links_left

    try:
        with driver_lock:  # Блокировка для безопасного создания драйвера
            driver = uc.Chrome(options=get_options(is_headless=False))


        desc_page = get_product_description_page(driver,link)

        # Генерация уникального имени файла

        with open(filename, 'w', encoding='utf-8') as f:
            parsed_data = parse_product_description(desc_page, link)
            f.write(parsed_data)

        links_left-=1

        print(f"Links left: {links_left}")
    except Exception as e:
        print(f"Error in {link}: {str(e)}")
    finally:
        if 'driver' in locals():
            driver.quit()
#"https://www.dns-shop.ru/product/8b991fcbf28ced20/korpus-cougar-duoface-rgb-385zd100011-cernyj/characteristics"

colors = {'koricnevyj','serebrisyj','goluboj','cernyj','belyj','bezevyj','krasnyj'}

def is_colored(link: str) -> str:
    color = link.split('-')[-1].split('/')[0]
    if color in colors:
        return '-'.join(link.split('-')[0:-1])
    else:
        return link


def remove_color_duplicates(raw_links: set) -> list:
    prefixes = set(map(lambda x: is_colored(x), raw_links))
    print(prefixes)
    prefixes_dict = {k: None for k in prefixes}
    print(prefixes_dict)
    for raw_link in raw_links:
        pref = is_colored(raw_link)
        print(prefixes_dict)
        if prefixes_dict[pref] is not None:
            continue
        else:
            prefixes_dict[pref] = raw_link
    result =list(prefixes_dict.values())
    print(f'removed {len(raw_links)-len(result)} duplicate color link/s')
    return result




if __name__ == "__main__":

    # количество потоков/браузеров
    max_processes = 10
    driver_ = uc.Chrome(options=get_options(is_headless=False))
    driver_.start_session()
    # если оно встретит такую ссылку на странице
    # то поиск ссылок остановится
    # *в списке будут все ссылки до этого
    # это на будущее чтоб новинки тырить
    # и запоминать на какой закончили
    # и не тырить то что есть и так
    stop_link = "sosi maksim"
    links = get_page_links_enhanced(driver_, stop_link,3)
    print(links)
    links_left = len(links)
    print(links_left)
    print(OPOSSING)
    print("Сейчас он будет опоссничать.....")
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_processes) as executor:
        executor.map(process_product,links)
    driver_.quit()



    # for link in links:
    #     desc_page = get_product_description_page(driver_,link)
    #     print(desc_page)
    #     with open(f"{link.split('/')[-3]}.txt",'w',encoding="utf-8",errors='ignore') as f:
    #         f.write(parse_product_description(desc_page,link))
    # driver_.quit()


